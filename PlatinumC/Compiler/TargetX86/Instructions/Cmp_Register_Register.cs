using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Cmp_Register_Register : X86Instruction
    {
        public X86Register Operand1 { get; set; }
        public X86Register Operand2 { get; set; }
        public Cmp_Register_Register(X86Register operand1, X86Register operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"cmp {Operand1}, {Operand2}";
        }
    }

    public class Cmp_Byte_Byte : X86Instruction
    {
        public X86ByteRegister Operand1 { get; set; }
        public X86ByteRegister Operand2 { get; set; }
        public Cmp_Byte_Byte(X86ByteRegister operand1, X86ByteRegister operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"cmp {Operand1}, {Operand2}";
        }
    }

    public class Cmp_Register_Immediate : X86Instruction
    {
        public X86Register Operand1 { get; set; }
        public int Operand2 { get; set; }
        public Cmp_Register_Immediate(X86Register operand1, int operand2)
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
