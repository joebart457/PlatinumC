using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Mov_Register_Offset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Mov_Register_Offset(X86Register destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Mov_Offset_Register : X86Instruction
    {
        public RegisterOffset Destination { get; set; }
        public X86Register Source { get; set; }
        public Mov_Offset_Register(RegisterOffset destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Mov_Register_Register : X86Instruction
    {
        public X86Register Destination { get; set; }
        public X86Register Source { get; set; }
        public Mov_Register_Register(X86Register destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Mov_Register_Immediate : X86Instruction
    {
        public X86Register Destination { get; set; }
        public int ImmediateValue { get; set; }
        public Mov_Register_Immediate(X86Register destination, int immediateValue)
        {
            Destination = destination;
            ImmediateValue = immediateValue;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {ImmediateValue}";
        }
    }

    public class Mov_Offset_Immediate : X86Instruction
    {
        public RegisterOffset Destination { get; set; }
        public int Immediate { get; set; }
        public Mov_Offset_Immediate(RegisterOffset destination, int immediate)
        {
            Destination = destination;
            Immediate = immediate;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Immediate}";
        }
    }

    public class Mov_SymbolOffset_Register : X86Instruction
    {
        public SymbolOffset Destination { get; set; }
        public X86Register Source { get; set; }
        public Mov_SymbolOffset_Register(SymbolOffset destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    }

    public class Mov_SymbolOffset_Immediate : X86Instruction
    {
        public SymbolOffset Destination { get; set; }
        public int ImmediateValue { get; set; }
        public Mov_SymbolOffset_Immediate(SymbolOffset destination, int immediateValue)
        {
            Destination = destination;
            ImmediateValue = immediateValue;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {ImmediateValue}";
        }
    }

    public class Mov_SymbolOffset_Register__Byte : X86Instruction
    {
        public SymbolOffset Destination { get; set; }
        public X86ByteRegister Source { get; set; }
        public Mov_SymbolOffset_Register__Byte(SymbolOffset destination, X86ByteRegister source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"mov {Destination}, {Source}";
        }
    } 
}
