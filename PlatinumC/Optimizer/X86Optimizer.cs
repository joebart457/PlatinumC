using PlatinumC.Compiler;
using PlatinumC.Compiler.TargetX86.Instructions;

namespace PlatinumC.Optimizer
{
    public class X86Optimizer
    {

        private class RegisterOffsetOrImmediate
        {
            public int? ImmediateValue { get; set; }
            public RegisterOffset? RegisterOffset { get; set; }
            public RegisterOffsetOrImmediate(int? immediateValue, RegisterOffset? registerOffset)
            {
                ImmediateValue = immediateValue;
                RegisterOffset = registerOffset;
            }

            public bool IsImmediate => ImmediateValue != null;
            public bool IsRegisterOffset => RegisterOffset != null;

            public override bool Equals(object? obj)
            {
                if (obj is RegisterOffsetOrImmediate offsetOrImmediate)
                {
                    if (IsImmediate && offsetOrImmediate.IsImmediate) return ImmediateValue == offsetOrImmediate.ImmediateValue;
                    if (IsRegisterOffset && offsetOrImmediate.IsRegisterOffset) return RegisterOffset!.Equals(offsetOrImmediate.RegisterOffset);
                    return false;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return 1;
            }

            public static RegisterOffsetOrImmediate Create(int immediate) => new RegisterOffsetOrImmediate(immediate, null);
            public static RegisterOffsetOrImmediate Create(RegisterOffset registerOffset) => new RegisterOffsetOrImmediate(null, registerOffset);
        }
        public CompilationResult Optimize(CompilationResult compilationResult)
        {
            for (int i = 0; i < compilationResult.CompilationOptions.OptimizationPasses; i++)
            {
                MakeOpimizationPass2(compilationResult);
            }
            return compilationResult;
        }

        private Dictionary<X86Register, RegisterOffsetOrImmediate> _registerValues = new();
        private Dictionary<RegisterOffset, RegisterOffsetOrImmediate> _memoryMap = new();
        private void WipeRegister(X86Register register)
        {
            _registerValues.Remove(register);
            WipeMemory(register);
            foreach(var key in _registerValues.Keys)
            {
                if (_registerValues[key].IsRegisterOffset && _registerValues[key].RegisterOffset!.Register == register)
                {
                    _registerValues.Remove(key);
                }
            }
        }

        private void WipeMemory(RegisterOffset registerOffset)
        {
            _memoryMap.Remove(registerOffset);
            foreach(var key in _memoryMap.Keys)
            {
                if (_memoryMap[key].IsRegisterOffset && _memoryMap[key].RegisterOffset!.Equals(registerOffset))
                {
                    _memoryMap.Remove(key);
                }
            }
        }

        private void WipeMemory(X86Register register)
        {
            
            foreach (var key in _memoryMap.Keys)
            {
                if (key.Register == register || (_memoryMap[key].IsRegisterOffset && _memoryMap[key].RegisterOffset!.Register == register))
                {
                    _memoryMap.Remove(key);
                }
            }
        }

        private void SetTopOfStack(RegisterOffsetOrImmediate? value, bool wipeRegister = true)
        {
            if (wipeRegister) WipeRegister(X86Register.esp);
            if (value != null) _memoryMap[Offset.Create(X86Register.esp, 0, true)] = value;
        }

        private X86Instruction TrackInstruction(X86Instruction instruction)
        {
            if (instruction is Push push)
            {
                if (!push.IsIndirect && _registerValues.TryGetValue(push.Register, out var registerValue))
                {
                    SetTopOfStack(registerValue);
                } else SetTopOfStack(RegisterOffsetOrImmediate.Create(Offset.Create(push.Register, 0, true)));
            }
            else if (instruction is Push_Offset push_offset)
            {
                WipeRegister(X86Register.esp);
                SetTopOfStack(RegisterOffsetOrImmediate.Create(push_offset.Offset));
            }
            else if (instruction is Push_Immediate<int> push_int)
            {
                SetTopOfStack(RegisterOffsetOrImmediate.Create(push_int.Immediate));
            }
            else if (instruction is Push_Immediate<float>)
            {
                SetTopOfStack(null);
            }
            else if (instruction is Push_Address)
            {
                SetTopOfStack(null);
            }
            else if (instruction is Pop_Register pop_register)
            {
                WipeRegister(pop_register.Destination);
                
                if (_memoryMap.TryGetValue(Offset.Create(X86Register.esp, 0, true), out var tosValue) && pop_register.Destination != X86Register.esp)
                {         
                    _registerValues[pop_register.Destination] = tosValue;
                }
                
                SetTopOfStack(null);

            }
            else if (instruction is Jmp)
            {
                _registerValues.Clear();
                SetTopOfStack(null);
            }
            else if (instruction is Call)
            {
                _registerValues.Clear();
                SetTopOfStack(null);
            }
            else if (instruction is Ret)
            {
                _registerValues.Clear();
                SetTopOfStack(null);
            }
            else if (instruction is Ret_Immediate)
            {
                _registerValues.Clear();
                SetTopOfStack(null);
            }
            else if (instruction is Label)
            {
                _registerValues.Clear();
                SetTopOfStack(null);
            }
            else if (instruction is Add_Register_Immediate add_Register_Immediate)
            {
                WipeRegister(add_Register_Immediate.Destination);
            }
            else if (instruction is Add_Register_Offset add_Register_Offset)
            {
                WipeRegister(add_Register_Offset.Destination);
            }
            else if (instruction is Add_Register_Register add_Register_Register)
            {
                WipeRegister(add_Register_Register.Destination);
            }
            else if (instruction is Sub sub)
            {
                WipeRegister(sub.Destination);
            }
            else if (instruction is Sub_Register_Register sub_Register_Register)
            {
                WipeRegister(sub_Register_Register.Destination);
            }
            else if (instruction is IDiv idiv)
            {
                WipeRegister(X86Register.eax);
                WipeRegister(X86Register.edx);
            }
            else if (instruction is Cdq cdq)
            {
                WipeRegister(X86Register.edx);
            }
            else if (instruction is IMul imul)
            {
                WipeRegister(imul.Destination);
            }
            else if (instruction is Lea lea)
            {
                WipeRegister(lea.Destination);
            }
            else if (instruction is IMul_Immediate imul_Immediate)
            {
                WipeRegister(imul_Immediate.Destination);
            }
            else if (instruction is Mov_Offset_Register mov_offset_register_esp && mov_offset_register_esp.Destination.IsIndirect && mov_offset_register_esp.Destination.Register == X86Register.esp && mov_offset_register_esp.Destination.Offset == 0)
            {
                // if instruciton is mov [esp], <reg>
                // set the top of the stack

                if (_registerValues.TryGetValue(mov_offset_register_esp.Source, out var registerValue))
                {
                    SetTopOfStack(registerValue, false); // do not wipe esp because esp is preserved
                    WipeMemory(mov_offset_register_esp.Destination);
                    _memoryMap[mov_offset_register_esp.Destination] = registerValue;
                }
            }
            else if (instruction is Mov_Offset_Register mov_offset_register)
            {
                WipeMemory(mov_offset_register.Destination);
                _registerValues.TryGetValue(mov_offset_register.Source, out var registerValue);
                var destination = RegisterOffsetOrImmediate.Create(mov_offset_register.Destination);
                foreach(var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination))
                    {
                        if (registerValue != null)
                        {
                            _registerValues[key] = registerValue;
                            _memoryMap[mov_offset_register.Destination] = registerValue;
                        }
                        else _registerValues.Remove(key);
                    }
                }
            }
            else if (instruction is Mov_Offset_Immediate mov_Offset_Immediate)
            {
                WipeMemory(mov_Offset_Immediate.Destination);
                _memoryMap[mov_Offset_Immediate.Destination] = RegisterOffsetOrImmediate.Create(mov_Offset_Immediate.Immediate);
                var destination = RegisterOffsetOrImmediate.Create(mov_Offset_Immediate.Destination);
                foreach (var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination)) _registerValues.Remove(key);
                }
                if (mov_Offset_Immediate.Destination.IsIndirect && mov_Offset_Immediate.Destination.Register == X86Register.esp && mov_Offset_Immediate.Destination.Offset == 0)
                    SetTopOfStack(RegisterOffsetOrImmediate.Create(mov_Offset_Immediate.Immediate));

            }
            else if (instruction is Fstp fstp)
            {
                WipeMemory(fstp.Destination);
                var destination = RegisterOffsetOrImmediate.Create(fstp.Destination);
                foreach (var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination)) _registerValues.Remove(key);
                }
            }
            else if (instruction is Mov_Register_Immediate register_immediate)
            {
                 WipeRegister(register_immediate.Destination);
                _registerValues[register_immediate.Destination] = RegisterOffsetOrImmediate.Create(register_immediate.ImmediateValue);
            }
            else if (instruction is Mov_Register_Offset mov_Register_Offset)
            {
                WipeRegister(mov_Register_Offset.Destination);
                _registerValues[mov_Register_Offset.Destination] = RegisterOffsetOrImmediate.Create(mov_Register_Offset.Source);
            }
            else if (instruction is Mov_Register_Register mov_Register_Register)
            {
                WipeRegister(mov_Register_Register.Destination);
                if (_registerValues.TryGetValue(mov_Register_Register.Destination, out var registerValue))
                    _registerValues[mov_Register_Register.Destination] = registerValue;
            }

            return instruction;
        }


        private CompilationResult MakeOpimizationPass2(CompilationResult compilationResult)
        {
            _registerValues.Clear();
            foreach (var fn in compilationResult.FunctionData)
            {
                var optimizedInstructions = new List<X86Instruction>();
                for (int i = 0; i < fn.Instructions.Count; i++)
                {
                    var instruction = fn.Instructions[i];
                    
                    if (instruction is Mov_Register_Offset mov_Register_Offset)
                    {
                        if (_registerValues.TryGetValue(mov_Register_Offset.Destination, out var registerValue))
                        {
                            // Test for the following scenario
                            // mov eax, [ebp+4]
                            // ...
                            // mov eax, [ebp +4]
                            // optimization:
                            // mov eax, [ebp+4]
                            // ...              (remove unncessary duplicate instruction)
                            if (registerValue.Equals(mov_Register_Offset.Source)) continue;
                        }

                        if (_memoryMap.TryGetValue(mov_Register_Offset.Source, out var valueAtMemory))
                        {
                            if (valueAtMemory.IsImmediate)
                            {
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Mov(mov_Register_Offset.Destination, valueAtMemory.ImmediateValue!.Value)));
                                continue;
                            }
                        }

                        // If the register is not referenced after the mov, it can be removed
                        if (!IsReferenced(fn.Instructions, i + 1, mov_Register_Offset.Destination)) continue;
                    }
                    else if (instruction is Mov_Offset_Register mov_Offset_Register)
                    {
                        if (_registerValues.TryGetValue(mov_Offset_Register.Source, out var registerValue))
                        {
                            _memoryMap.TryGetValue(mov_Offset_Register.Destination, out var valueAtDestination);
                            // Test for the following scenario
                            // mov [ebp-4], 7
                            // mov eax, 7
                            // mov [ebp-4], eax
                            // optimization:
                            // mov [ebp-4], 7
                            // ...              (eliminate unncessary instructions since value is already there)
                            if (valueAtDestination?.Equals(registerValue) == true) continue;

                            // Test for the following scenario
                            // mov [ebp-4], eax
                            // ...
                            // mov [ebp-4], eax
                            // optimization:
                            // mov [ebp-4], eax
                            // ...              (eliminate duplicate instruction)
                            if (registerValue.Equals(mov_Offset_Register.Destination)) continue;

                            if (registerValue.IsImmediate)
                            {
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Mov(mov_Offset_Register.Destination, registerValue.ImmediateValue!.Value)));
                                continue;
                            }
                        }
                        if (!IsLocalReferenced(fn.Instructions, i, mov_Offset_Register.Destination)) continue;
                    }
                    else if (instruction is Mov_Register_Immediate mov_Register_Immediate)
                    {
                        if (_registerValues.TryGetValue(mov_Register_Immediate.Destination, out var registerValue))
                        {
                            // Test for the following scenario
                            // mov eax, 1
                            // ...
                            // mov eax, 1
                            // optimization:
                            // mov eax, 1
                            // ...              (remove unncessary instruction)
                            if (registerValue.IsImmediate && mov_Register_Immediate.ImmediateValue == registerValue.ImmediateValue) continue;
                        }

                        // If the register is not referenced after the mov, it can be removed
                        if (!IsReferenced(fn.Instructions, i + 1, mov_Register_Immediate.Destination)) continue;
                    }
                    else if (instruction is Mov_Register_Register mov_Register_Register)
                    {
                        if (_registerValues.TryGetValue(mov_Register_Register.Destination, out var destinationRegisterValue)
                            && _registerValues.TryGetValue(mov_Register_Register.Source, out var sourceRegisterValue))
                        {
                            // Test for the following scenario
                            // mov eax, [ebp-4]
                            // mov ebx, [ebp-4]
                            // mov eax, ebx
                            // optimization:
                            // mov eax, [ebp-4]
                            // mov ebx, [ebp-4]
                            // ...              (remove unncessary instruction)
                            if (destinationRegisterValue.Equals(sourceRegisterValue)) continue;
                        }
                        // If the register is not referenced after the mov, it can be removed
                        if (!IsReferenced(fn.Instructions, i + 1, mov_Register_Register.Destination)) continue;
                    }
                    else if (instruction is Mov_Offset_Immediate mov_Offset_Immediate)
                    {
                        if (!IsLocalReferenced(fn.Instructions, i + 1, mov_Offset_Immediate.Destination)) continue;
                    }
                    else if (instruction is Push push)
                    {

                        if (Peek(fn.Instructions, i + 1) is Pop_Register pop_register)
                        {
                            if (push.IsIndirect)
                            {
                                // Test for following scenario
                                // push [eax]
                                // pop ebx
                                // optimization:
                                // mov ebx, [eax]
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Mov(pop_register.Destination, Offset.Create(push.Register, 0, true))));
                                i++;
                                continue;
                            }
                            else if (push.Register != pop_register.Destination)
                            {
                                // Test for following scenario
                                // push eax
                                // pop ebx
                                // optimization:
                                // mov ebx, eax 
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Mov(pop_register.Destination, push.Register)));
                                i++;
                                continue;
                            }
                            else if (push.Register == pop_register.Destination)
                            {
                                // Test for the following scenario
                                // push eax
                                // pop eax
                                // optimization:
                                // --no instructions necessary --
                                i++;
                                continue;
                            }
                        }
                        else if (Peek(fn.Instructions, i + 1) is Mov_Register_Offset mov_register_offset)
                        {

                            if (!push.IsIndirect && push.Register == mov_register_offset.Destination && mov_register_offset.Source.Register == X86Register.esp && mov_register_offset.Source.IsIndirect && mov_register_offset.Source.Offset == 0)
                            {
                                // Test for the following scenario
                                // push eax
                                // mov eax, dword [esp]
                                // optimization:
                                // push eax
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Push(push.Register, false)));
                                i++;
                                continue;
                            }
                        }
                        else if (Peek(fn.Instructions, i + 1) is Add_Register_Immediate add_Register_Immediate && add_Register_Immediate.Value == 4 && add_Register_Immediate.Destination == X86Register.esp)
                        {
                            // essentially poping the stack to nowhere
                            i++;
                            continue;
                        }
                        if (push.IsIndirect)
                        {
                            var pushSource = RegisterOffsetOrImmediate.Create(Offset.Create(push.Register, 0, true));
                            var registerWithSameValueIndex = _registerValues.Keys.ToList().FindIndex(key => _registerValues[key].Equals(pushSource));
                            if (registerWithSameValueIndex != -1)
                            {
                                optimizedInstructions.Add(TrackInstruction(X86Instructions.Push(_registerValues.Keys.ElementAt(registerWithSameValueIndex), false)));
                                continue;
                            }        
                        }
                    }
                    else if (instruction is Push_Offset push_offset)
                    {
                        if (Peek(fn.Instructions, i + 1) is Add_Register_Immediate add_Register_Immediate && add_Register_Immediate.Value == 4 && add_Register_Immediate.Destination == X86Register.esp)
                        {
                            // essentially poping the stack to nowhere
                            i++;
                            continue;
                        }
                        var pushSource = RegisterOffsetOrImmediate.Create(push_offset.Offset);
                        var registerWithSameValueIndex = _registerValues.Keys.ToList().FindIndex(key => _registerValues[key].Equals(pushSource));
                        if (registerWithSameValueIndex != -1)
                        {
                            optimizedInstructions.Add(TrackInstruction(X86Instructions.Push(_registerValues.Keys.ElementAt(registerWithSameValueIndex), false)));
                            continue;
                        }

                        if (_memoryMap.TryGetValue(push_offset.Offset, out var valueAtMemory) && valueAtMemory.IsImmediate)
                        {
                            optimizedInstructions.Add(TrackInstruction(X86Instructions.Push(valueAtMemory.ImmediateValue!.Value)));
                            continue;
                        }
                    }
                    else if (instruction is Push_Immediate<int> push_immediate)
                    {
                        if (Peek(fn.Instructions, i + 1) is Pop_Register pop_register)
                        {
                            optimizedInstructions.Add(TrackInstruction(X86Instructions.Mov(pop_register.Destination, push_immediate.Immediate)));
                            i++;
                            continue;
                        }
                        else if (Peek(fn.Instructions, i + 1) is Add_Register_Immediate add_Register_Immediate && add_Register_Immediate.Value == 4 && add_Register_Immediate.Destination == X86Register.esp)
                        {
                            // essentially poping the stack to nowhere
                            i++;
                            continue;
                        }
                    }

                   
                    optimizedInstructions.Add(TrackInstruction(instruction));
                }

                fn.Instructions = optimizedInstructions;
                _registerValues.Clear();
            }

            return compilationResult;
        }


        private CompilationResult MakeOpimizationPass(CompilationResult compilationResult)
        {
            var registerValues = new Dictionary<X86Register, RegisterOffsetOrImmediate>();
            foreach (var fn in compilationResult.FunctionData)
            {
                X86Register? topOfStack = null;
                var optimizedInstructions = new List<X86Instruction>();
                for (int i = 0; i < fn.Instructions.Count; i++)
                {
                    var instruction = fn.Instructions[i];
                    if (instruction is Jmp)
                    {
                        registerValues.Clear();
                        topOfStack = null;
                    }
                    else if (instruction is Call)
                    {
                        registerValues.Clear();
                        topOfStack = null;
                    }
                    else if (instruction is Ret)
                    {
                        registerValues.Clear();
                        topOfStack = null;
                    }
                    else if (instruction is Ret_Immediate)
                    {
                        registerValues.Clear();
                        topOfStack = null;
                    }
                    else if (instruction is Label)
                    {
                        registerValues.Clear();
                        topOfStack = null;
                    }
                    else if (instruction is Pop_Register pop_Register)
                    {
                        if (topOfStack != null)
                        {
                            if (registerValues.TryGetValue(topOfStack.Value, out var registerValue))
                            {
                                if (registerValues.TryGetValue(topOfStack.Value, out var destinationRegisterValue))
                                {
                                    if (registerValue.Equals(destinationRegisterValue)) continue;
                                }
                            }
                        }
                        registerValues.Remove(pop_Register.Destination);
                    }
                    else if (instruction is Mov_Register_Offset mov_Register_Offset)
                    {
                        if (registerValues.TryGetValue(mov_Register_Offset.Destination, out var registerValue))
                        {
                            if (mov_Register_Offset.Source.Register != X86Register.esp && registerValue.Equals(mov_Register_Offset.Source)) continue;
                        }
                        registerValues[mov_Register_Offset.Destination] = RegisterOffsetOrImmediate.Create(mov_Register_Offset.Source);
                    }

                    else if (instruction is Mov_Register_Immediate mov_Register_Immediate)
                    {
                        if (registerValues.TryGetValue(mov_Register_Immediate.Destination, out var registerValue))
                        {
                            if (registerValue.IsImmediate && mov_Register_Immediate.ImmediateValue == registerValue.ImmediateValue) continue;
                        }
                        registerValues[mov_Register_Immediate.Destination] = RegisterOffsetOrImmediate.Create(mov_Register_Immediate.ImmediateValue);
                    }
                    else if (instruction is Push push)
                    {

                        if (Peek(fn.Instructions, i + 1) is Pop_Register pop_register)
                        {
                            if (push.IsIndirect)
                            {
                                // Test for following scenario
                                // push [eax]
                                // pop ebx
                                // optimization:
                                // mov ebx, [eax]
                                optimizedInstructions.Add(X86Instructions.Mov(pop_register.Destination, Offset.Create(push.Register, 0, true)));
                                registerValues[pop_register.Destination] = RegisterOffsetOrImmediate.Create(Offset.Create(push.Register, 0, true));
                                i++;
                                continue;
                            }
                            else if (push.Register != pop_register.Destination)
                            {
                                // Test for following scenario
                                // push eax
                                // pop ebx
                                // optimization:
                                // mov ebx, eax 
                                optimizedInstructions.Add(X86Instructions.Mov(pop_register.Destination, push.Register));
                                if (registerValues.TryGetValue(push.Register, out var registerValue)) registerValues[pop_register.Destination] = registerValue;
                                else registerValues.Remove(pop_register.Destination);
                                i++;
                                continue;
                            }
                            else if (push.Register == pop_register.Destination)
                            {
                                // Test for following scenario
                                // push eax
                                // pop eax
                                // optimization:
                                // --no instructions necessary --
                                i++;
                                continue;
                            }
                            else
                            {
                                topOfStack = push.Register;
                            }



                        }
                        else if (Peek(fn.Instructions, i + 1) is Mov_Register_Offset mov_register_offset)
                        {

                            if (!push.IsIndirect && push.Register == mov_register_offset.Destination && mov_register_offset.Source.Register == X86Register.esp && mov_register_offset.Source.IsIndirect && mov_register_offset.Source.Offset == 0)
                            {
                                // Test for following scenario
                                // push eax
                                // mov eax, dword [esp]
                                // optimization:
                                // push eax
                                optimizedInstructions.Add(X86Instructions.Push(push.Register, false));
                                i++;
                                continue;
                            }



                        }
                    }
                    else if (instruction is Push_Immediate<int> push_immediate)
                    {
                        if (Peek(fn.Instructions, i + 1) is Pop_Register pop_register)
                        {
                            optimizedInstructions.Add(X86Instructions.Mov(pop_register.Destination, push_immediate.Immediate));
                            registerValues[pop_register.Destination] = RegisterOffsetOrImmediate.Create(push_immediate.Immediate);
                            i++;
                            continue;
                        }
                    }
                    optimizedInstructions.Add(instruction);
                }

                fn.Instructions = optimizedInstructions;
                registerValues.Clear();
            }

            return compilationResult;
        }

        private X86Instruction? Peek(List<X86Instruction> instructions, int index)
        {
            if (index < instructions.Count) return instructions[index];
            return null;
        }

        private bool IsReferenced(List<X86Instruction> instructions, int index, X86Register register)
        {
            if (register == X86Register.esp) return true; // excliude stack operations
            if (index >= instructions.Count) return false;
            var instruction = instructions[index];
            if (instruction is Push push)
            {
                if (push.Register == register) return true;
            }
            else if (instruction is Push_Offset push_offset)
            {
                if (push_offset.Offset.Register == register) return true;
            }
            else if (instruction is Pop_Register pop_register)
            {
                return false;
            }
            else if (instruction is Jmp)
            {
                return false;
            }
            else if (instruction is Call)
            {
                // These registers are caller saved
                if (register == X86Register.eax || register == X86Register.ecx || register == X86Register.edx) return false;
            }
            else if (instruction is Ret)
            {
                if (register == X86Register.eax) return true;
                return false;
            }
            else if (instruction is Ret_Immediate)
            {
                if (register == X86Register.eax) return true;
                return false;
            }
            else if (instruction is Label)
            {

            }
            else if (instruction is Lea lea)
            {
                if (lea.Source.Register == register) return true;
                if (lea.Destination == register) return false;
            }
            else if (instruction is Add_Register_Immediate add_Register_Immediate)
            {
                if (add_Register_Immediate.Destination == register) return true;
            }
            else if (instruction is Add_Register_Offset add_Register_Offset)
            {
                if (add_Register_Offset.Destination == register || add_Register_Offset.Source.Register == register) return true;
            }
            else if (instruction is Add_Register_Register add_Register_Register)
            {
                if (add_Register_Register.Destination == register || add_Register_Register.Source == register) return true;
            }
            else if (instruction is Sub sub)
            {
                if (sub.Destination == register) return true;
            }
            else if (instruction is Sub_Register_Register sub_Register_Register)
            {
                if (sub_Register_Register.Destination == register || sub_Register_Register.Source == register) return true;
            }
            else if (instruction is IDiv idiv)
            {
                if (register == X86Register.eax || register == X86Register.edx) return true;
            }
            else if (instruction is Cdq cdq)
            {
                if (register == X86Register.eax) return true;
                if (register == X86Register.edx) return false;
            }
            else if (instruction is IMul imul)
            {
                if (imul.Destination == register || imul.Source == register) return true;

            }
            else if (instruction is IMul_Immediate imul_Immediate)
            {
                if (imul_Immediate.Destination == register) return true;
            }
            else if (instruction is Mov_Offset_Register mov_offset_register)
            {
                if (mov_offset_register.Destination.Register == register || mov_offset_register.Source == register) return true;
            }
            else if (instruction is Mov_Offset_Immediate mov_Offset_Immediate)
            {
                if (mov_Offset_Immediate.Destination.Register == register) return true;
            }
            else if (instruction is Fstp fstp)
            {
                if (fstp.Destination.Register == register) return true;
            }
            else if (instruction is Mov_Register_Immediate register_immediate)
            {
                if (register_immediate.Destination == register) return false;
            }
            else if (instruction is Mov_Register_Offset mov_Register_Offset)
            {
                if (mov_Register_Offset.Source.Register == register) return true;
                if (mov_Register_Offset.Destination == register) return false;
            }
            else if (instruction is Mov_Register_Register mov_Register_Register)
            {
                if (mov_Register_Register.Source == register) return true;
                if (mov_Register_Register.Destination == register) return false;
            }

            return IsReferenced(instructions, index + 1, register);
        }

        private bool IsLocalReferenced(List<X86Instruction> instructions, int index, RegisterOffset registerOffset, int? endIndex = null, HashSet<string>? exploredLabels = null)
        {
            // This function assumes no arithmetic or other manipulation is done to ebp 
            // and ebp is only ever used for local variable addressing
            // this also ignores function preludes, etc
            // ie mov esp, ebp
            //    pop ebp
            if (registerOffset.Register != X86Register.ebp) return true; // exclude non-local variable operations
            if (endIndex == null) endIndex = instructions.Count;
            if (exploredLabels == null) exploredLabels = new();
            if (index >= endIndex) return false;
            var instruction = instructions[index];

            if (instruction is Push_Offset push_offset)
            {
                if (push_offset.Offset.Equals(registerOffset)) return true;
            }
            else if (instruction is Label label)
            {
                if (exploredLabels.Contains(label.Text)) return false;
                exploredLabels.Add(label.Text);
            }
            else if (instruction is Jmp jump && jump.Emit().StartsWith("jmp"))
            {
                // if it is not a conditional jump
                if (exploredLabels.Contains(jump.Label)) return false;
                var labelIndex = instructions.FindIndex(x => x is Label label && label.Text == jump.Label);
                if (labelIndex != -1)
                {
                    if (labelIndex < index)
                    {
                        // we are jumping backwards
                        return IsLocalReferenced(instructions, labelIndex, registerOffset, index, exploredLabels);
                    }
                    else return IsLocalReferenced(instructions, labelIndex, registerOffset, instructions.Count, exploredLabels);
                }
                else return true;
            }
            else if (instruction is Jmp jmp && !exploredLabels.Contains(jmp.Label))
            {
                // the following allows for this check to occur from the middle of a function instead of just the beginning
                var labelIndex = instructions.FindIndex(x => x is Label label && label.Text == jmp.Label);
                if (labelIndex != -1)
                {
                    if (labelIndex < index)
                    {
                        // we are jumping backwards
                        if (IsLocalReferenced(instructions, labelIndex, registerOffset, index, exploredLabels)) return true;
                    }
                    else
                    {
                        endIndex = labelIndex;
                        // we are jumping forwards
                        if (IsLocalReferenced(instructions, labelIndex, registerOffset, instructions.Count, exploredLabels)) return true;
                    }
                }
                else
                {
                    // If we cannot find the label, to be safe, we must assume the local is referenced
                    return true;
                }
            }
            else if (instruction is Ret ret || instruction is Ret_Immediate ret_Immediate)
            {
                return false;
            }
            else if (instruction is Pop_Register pop_register)
            {
                // nothing is done here, see above note: pop ebp is ignored
            }
            else if (instruction is Add_Register_Offset add_Register_Offset)
            {
                if (add_Register_Offset.Source.Equals(registerOffset)) return true;
            }
            else if (instruction is Mov_Offset_Register mov_offset_register)
            {
                if (mov_offset_register.Destination.Equals(registerOffset)) return true;
            }
            else if (instruction is Mov_Offset_Immediate mov_Offset_Immediate)
            {
                if (mov_Offset_Immediate.Destination.Equals(registerOffset)) return true;
            }
            else if (instruction is Fstp fstp)
            {
                if (fstp.Destination.Equals(registerOffset)) return true;
            }
            else if (instruction is Mov_Register_Offset mov_Register_Offset)
            {
                if (mov_Register_Offset.Source.Equals(registerOffset)) return true;
            }
            else if (instruction is Lea lea)
            {
                if (lea.Source.Equals(registerOffset)) return true;
            }

            return IsLocalReferenced(instructions, index + 1, registerOffset, endIndex, exploredLabels);
        }

    }
}
