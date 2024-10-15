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

        private RegisterOffsetOrImmediate? _tosValue; // Top of Stack Value
        private Dictionary<X86Register, RegisterOffsetOrImmediate> _registerValues = new();

        private void WipeRegister(X86Register register)
        {
            _registerValues.Remove(register);
            foreach(var key in _registerValues.Keys)
            {
                if (_registerValues[key].IsRegisterOffset && _registerValues[key].RegisterOffset!.Register == register)
                {
                    _registerValues.Remove(key);
                }
            }
        }
        private X86Instruction TrackInstruction(X86Instruction instruction)
        {
            if (instruction is Push push)
            {
                WipeRegister(X86Register.esp);
                if (!push.IsIndirect)
                {
                    if (_registerValues.TryGetValue(push.Register, out var registerValue)) _tosValue = registerValue;
                }
                else _tosValue = RegisterOffsetOrImmediate.Create(Offset.Create(push.Register, 0, true));
            }
            else if (instruction is Push_Offset push_offset)
            {
                WipeRegister(X86Register.esp);
                _tosValue = RegisterOffsetOrImmediate.Create(push_offset.Offset);
            }
            else if (instruction is Pop_Register pop_register)
            {
                WipeRegister(pop_register.Destination);
                if (_tosValue != null && pop_register.Destination != X86Register.esp)
                {         
                    _registerValues[pop_register.Destination] = _tosValue;
                }
                
                WipeRegister(X86Register.esp);

            }
            else if (instruction is Jmp)
            {
                _registerValues.Clear();
                _tosValue = null;
            }
            else if (instruction is Call)
            {
                _registerValues.Clear();
                _tosValue = null;
            }
            else if (instruction is Ret)
            {
                _registerValues.Clear();
                _tosValue = null;
            }
            else if (instruction is Ret_Immediate)
            {
                _registerValues.Clear();
                _tosValue = null;
            }
            else if (instruction is Label)
            {
                _registerValues.Clear();
                _tosValue = null;
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
            else if (instruction is IMul_Immediate imul_Immediate)
            {
                WipeRegister(imul_Immediate.Destination);
            }
            else if (instruction is Mov_Offset_Register mov_offset_register_esp && mov_offset_register_esp.Destination.IsIndirect && mov_offset_register_esp.Destination.Register == X86Register.esp && mov_offset_register_esp.Destination.Offset == 0)
            {
                // if instruciton is mov [esp], <reg>
                // set the top of the stack

                if (_registerValues.TryGetValue(mov_offset_register_esp.Source, out var registerValue))
                    _tosValue = registerValue;
            }
            else if (instruction is Mov_Offset_Register mov_offset_register)
            {
                _registerValues.TryGetValue(mov_offset_register.Source, out var registerValue);
                var destination = RegisterOffsetOrImmediate.Create(mov_offset_register.Destination);
                foreach(var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination))
                    {
                        if (registerValue != null) _registerValues[key] = registerValue;
                        else _registerValues.Remove(key);
                    }
                }
            }
            else if (instruction is Mov_Offset_Immediate mov_Offset_Immediate)
            {
                var destination = RegisterOffsetOrImmediate.Create(mov_Offset_Immediate.Destination);
                foreach (var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination)) _registerValues.Remove(key);
                }
                if (mov_Offset_Immediate.Destination.IsIndirect && mov_Offset_Immediate.Destination.Register == X86Register.esp && mov_Offset_Immediate.Destination.Offset == 0)
                    _tosValue = RegisterOffsetOrImmediate.Create(mov_Offset_Immediate.Immediate);
            }
            else if (instruction is Fstp fstp)
            {
                var destination = RegisterOffsetOrImmediate.Create(fstp.Destination);
                foreach (var key in _registerValues.Keys)
                {
                    if (_registerValues[key].Equals(destination)) _registerValues.Remove(key);
                }
                if (fstp.Destination.IsIndirect && fstp.Destination.Register == X86Register.esp && fstp.Destination.Offset == 0)
                    _tosValue = null;
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
            _tosValue = null;
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
                    }
                    else if (instruction is Mov_Offset_Register mov_Offset_Register)
                    {
                        if (_registerValues.TryGetValue(mov_Offset_Register.Source, out var registerValue))
                        {
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
                    }else if (instruction is Push_Offset push_offset)
                    {
                        var pushSource = RegisterOffsetOrImmediate.Create(push_offset.Offset);
                        var registerWithSameValueIndex = _registerValues.Keys.ToList().FindIndex(key => _registerValues[key].Equals(pushSource));
                        if (registerWithSameValueIndex != -1)
                        {
                            optimizedInstructions.Add(TrackInstruction(X86Instructions.Push(_registerValues.Keys.ElementAt(registerWithSameValueIndex), false)));
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
                    }

                   
                    optimizedInstructions.Add(TrackInstruction(instruction));
                }

                fn.Instructions = optimizedInstructions;
                _registerValues.Clear();
                _tosValue = null;
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

    }
}
