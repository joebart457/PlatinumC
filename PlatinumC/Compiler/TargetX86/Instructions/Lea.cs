
namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Lea_Register_Offset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Lea_Register_Offset(X86Register destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"lea {Destination}, {Source}";
        }
    }

    public class Lea_Register_SymbolOffset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public SymbolOffset Source { get; set; }
        public Lea_Register_SymbolOffset(X86Register destination, SymbolOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"lea {Destination}, {Source}";
        }
    }
}
