using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Xor_Register_Register : X86Instruction
    {
        public X86Register Destination { get; set; }
        public X86Register Source { get; set; }

        public Xor_Register_Register(X86Register destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"xor {Destination}, {Source}";
        }
    }

    public class Xor_Register_Register__Byte : X86Instruction
    {
        public X86ByteRegister Destination { get; set; }
        public X86ByteRegister Source { get; set; }

        public Xor_Register_Register__Byte(X86ByteRegister destination, X86ByteRegister source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"xor {Destination}, {Source}";
        }
    }
}
