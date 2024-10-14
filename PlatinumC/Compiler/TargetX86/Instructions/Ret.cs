using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Ret : X86Instruction
    {
        public override string Emit()
        {
            return $"ret";
        }
    }

    public class Ret_Immediate : X86Instruction
    {
        public int ImmediateValue { get; set; }

        public Ret_Immediate(int immediateValue)
        {
            ImmediateValue = immediateValue;
        }

        public override string Emit()
        {
            return $"ret {ImmediateValue}";
        }
    }
}
