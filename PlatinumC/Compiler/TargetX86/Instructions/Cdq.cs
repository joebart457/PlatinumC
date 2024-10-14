using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Cdq : X86Instruction
    {
        public override string Emit()
        {
            return $"cdq";
        }
    }
}
