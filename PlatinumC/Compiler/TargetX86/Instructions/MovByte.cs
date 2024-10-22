using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Mov_Offset_Register__Byte : X86Instruction
    {
        public RegisterOffset Destination { get; set; }
        public X86ByteRegister Source { get; set; }
        public Mov_Offset_Register__Byte(RegisterOffset destination, X86ByteRegister source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Mov_Register_Offset__Byte : X86Instruction
    {
        public X86ByteRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Mov_Register_Offset__Byte(X86ByteRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Movsx_Register_Offset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset_Byte Source { get; set; }
        public Movsx_Register_Offset(X86Register destination, RegisterOffset_Byte source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"movsx {Destination}, {Source}";
        }
    }

    public class Movzx : X86Instruction
    {
        public X86Register Destination { get; set; }
        public X86ByteRegister Source { get; set; }
        public Movzx(X86Register destination, X86ByteRegister source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"movzx {Destination}, {Source}";
        }
    }

    public class Mov_Register_Immediate__Byte : X86Instruction
    {
        public X86ByteRegister Destination { get; set; }
        public byte ImmediateValue { get; set; }
        public Mov_Register_Immediate__Byte(X86ByteRegister destination, byte immediateValue)
        {
            Destination = destination;
            ImmediateValue = immediateValue;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {ImmediateValue}";
        }
    }
}