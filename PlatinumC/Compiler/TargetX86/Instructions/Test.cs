using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Test : X86Instruction
    {
        public X86Register Operand1 { get; set; }
        public X86Register Operand2 { get; set; }
        public Test(X86Register operand1, X86Register operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"test {Operand1}, {Operand2}";
        }
    }

    public class Test_Offset : X86Instruction
    {
        public X86Register Operand1 { get; set; }
        public RegisterOffset Operand2 { get; set; }
        public Test_Offset(X86Register operand1, RegisterOffset operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"test {Operand1}, {Operand2}";
        }
    }
}
