using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class IDiv_Offset : X86Instruction
    {
        public RegisterOffset Divisor { get; set; }

        public IDiv_Offset(RegisterOffset divisor)
        {
            Divisor = divisor;
        }

        public override string Emit()
        {
            return $"idiv {Divisor}";
        }
    }
}
