
namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Lea : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Lea(X86Register destination, RegisterOffset source)
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
