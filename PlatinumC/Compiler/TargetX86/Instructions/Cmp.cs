using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Cmp : X86Instruction
    {
        public X86Register Operand1 { get; set; }
        public X86Register Operand2 { get; set; }
        public Cmp(X86Register operand1, X86Register operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"cmp {Operand1}, {Operand2}";
        }
    }
}
