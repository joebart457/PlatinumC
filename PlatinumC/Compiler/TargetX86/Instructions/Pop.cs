using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Pop_Register : X86Instruction
    {
        public X86Register Destination { get; set; }

        public Pop_Register(X86Register destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"pop {Destination}";
        }
    }
}
