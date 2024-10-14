using PlatinumC.Compiler;
using PlatinumC.Compiler.TargetX86.Instructions;

namespace PlatinumC.Optimizer
{
    public class X86Optimizer
    {
        public CompilationResult Optimize(CompilationResult compilationResult)
        {
            foreach(var fn in compilationResult.FunctionData)
            {
                var optimizedInstructions = new List<X86Instruction>();
                for(int i = 0; i < fn.Instructions.Count; i++)
                {
                    var instruction = fn.Instructions[i];
                    if (instruction is Push push)
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
                        }
                    }
                    optimizedInstructions.Add(instruction);
                }
                fn.Instructions = optimizedInstructions;
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
