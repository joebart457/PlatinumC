using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Add_Register_Immediate : X86Instruction
    {
        public X86Register Destination { get; set; }
        public int ValueToAdd { get; set; }

        public Add_Register_Immediate(X86Register destination, int valueToAdd)
        {
            Destination = destination;
            ValueToAdd = valueToAdd;
        }

        public override string Emit()
        {
            return $"add {Destination}, {ValueToAdd}";
        }
    }

    public class Add_Register_Register: X86Instruction
    {
        public X86Register Destination { get; set; }
        public X86Register Source { get; set; }

        public Add_Register_Register(X86Register destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"add {Destination}, {Source}";
        }
    }

    public class Add_Register_Offset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset Source { get; set; }

        public Add_Register_Offset(X86Register destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"add {Destination}, {Source}";
        }
    }
}
