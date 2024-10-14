using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Sub : X86Instruction
    {
        public X86Register Destination { get; set; }
        public int ValueToSubtract { get; set; }
        public Sub(X86Register destination, int valueToSubtract)
        {
            Destination = destination;
            ValueToSubtract = valueToSubtract;
        }

        public override string Emit()
        {
            return $"sub {Destination}, {ValueToSubtract}";
        }
    }

    public class Sub_Register_Register : X86Instruction
    {
        public X86Register Destination { get; set; }
        public X86Register Source { get; set; }

        public Sub_Register_Register(X86Register destination, X86Register source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"sub {Destination}, {Source}";
        }
    }
}
