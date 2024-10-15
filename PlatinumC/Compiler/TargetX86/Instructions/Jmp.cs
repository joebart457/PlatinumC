using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Jmp : X86Instruction
    {
        public string Label { get; set; }

        public Jmp(string label)
        {
            Label = label;
        }
        public override string Emit()
        {
            return $"jmp {Label}";
        }
    }

    public class JmpGt : Jmp
    {
        public JmpGt(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jg {Label}";
        }
    }

    public class JmpLt : Jmp
    {
        public JmpLt(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jl {Label}";
        }
    }

    public class JmpGte : Jmp
    {

        public JmpGte(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jge {Label}";
        }
    }

    public class JmpLte : Jmp
    {
        public JmpLte(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jle {Label}";
        }
    }

    public class JmpEq : Jmp
    {
        public JmpEq(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"je {Label}";
        }
    }

    public class JmpNeq : Jmp
    {
        public JmpNeq(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jne {Label}";
        }
    }

    public class Jz : Jmp
    {
        public Jz(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jz {Label}";
        }
    }

    public class Jnz : Jmp
    {
        public Jnz(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jnz {Label}";
        }
    }

    public class Js : Jmp
    {
        public Js(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"js {Label}";
        }
    }

    public class Jns : Jmp
    {
        public Jns(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jns {Label}";
        }
    }

    public class Ja : Jmp
    {
        public Ja(string label): base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"ja {Label}";
        }
    }

    public class Jae : Jmp
    {
        public Jae(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jae {Label}";
        }
    }

    public class Jb : Jmp
    {
        public Jb(string label) : base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jb {Label}";
        }
    }

    public class Jbe : Jmp
    {

        public Jbe(string label): base(label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jbe {Label}";
        }
    }
}
