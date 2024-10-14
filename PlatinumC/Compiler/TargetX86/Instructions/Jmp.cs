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

    public class JmpGt : X86Instruction
    {
        public string Label { get; set; }

        public JmpGt(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jg {Label}";
        }
    }

    public class JmpLt : X86Instruction
    {
        public string Label { get; set; }

        public JmpLt(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jl {Label}";
        }
    }

    public class JmpGte : X86Instruction
    {
        public string Label { get; set; }

        public JmpGte(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jge {Label}";
        }
    }

    public class JmpLte : X86Instruction
    {
        public string Label { get; set; }

        public JmpLte(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jle {Label}";
        }
    }

    public class JmpEq : X86Instruction
    {
        public string Label { get; set; }

        public JmpEq(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"je {Label}";
        }
    }

    public class JmpNeq : X86Instruction
    {
        public string Label { get; set; }

        public JmpNeq(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jne {Label}";
        }
    }

    public class Jz : X86Instruction
    {
        public string Label { get; set; }

        public Jz(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jz {Label}";
        }
    }

    public class Jnz : X86Instruction
    {
        public string Label { get; set; }

        public Jnz(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jnz {Label}";
        }
    }

    public class Js : X86Instruction
    {
        public string Label { get; set; }

        public Js(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"js {Label}";
        }
    }

    public class Jns : X86Instruction
    {
        public string Label { get; set; }

        public Jns(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jns {Label}";
        }
    }

    public class Ja : X86Instruction
    {
        public string Label { get; set; }

        public Ja(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"ja {Label}";
        }
    }

    public class Jae : X86Instruction
    {
        public string Label { get; set; }

        public Jae(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jae {Label}";
        }
    }

    public class Jb : X86Instruction
    {
        public string Label { get; set; }

        public Jb(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jb {Label}";
        }
    }

    public class Jbe : X86Instruction
    {
        public string Label { get; set; }

        public Jbe(string label)
        {
            Label = label;
        }

        public override string Emit()
        {
            return $"jbe {Label}";
        }
    }
}
