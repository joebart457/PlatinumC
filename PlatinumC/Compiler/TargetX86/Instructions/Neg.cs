using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Neg_Offset : X86Instruction
    {
        public RegisterOffset Operand { get; set; }

        public Neg_Offset(RegisterOffset operand)
        {
            Operand = operand;
        }

        public override string Emit()
        {
            return $"neg {Operand}";
        }
    }
}
