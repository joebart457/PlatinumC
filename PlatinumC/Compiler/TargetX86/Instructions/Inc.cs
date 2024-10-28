using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Inc_Register : X86Instruction
    {
        public X86Register Destination { get; set; }

        public Inc_Register(X86Register destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"inc {Destination}";
        }
    }
    public class Dec_Register : X86Instruction
    {
        public X86Register Destination { get; set; }

        public Dec_Register(X86Register destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"dec {Destination}";
        }
    }

    public class Inc_Offset : X86Instruction
    {
        public RegisterOffset Destination { get; set; }

        public Inc_Offset(RegisterOffset destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"inc {Destination}";
        }
    }

    public class Dec_Offset : X86Instruction
    {
        public RegisterOffset Destination { get; set; }

        public Dec_Offset(RegisterOffset destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"dec {Destination}";
        }
    }
}
