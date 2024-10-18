using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Fstp : X86Instruction
    {
        public RegisterOffset Destination { get; set; }

        public Fstp(RegisterOffset destination)
        {
            Destination = destination;
        }

        public override string Emit()
        {
            return $"fstp {Destination}";
        }
    }

    public class Fstp_Register : X86Instruction
    {
        public X87Register Register { get; set; }

        public Fstp_Register(X87Register register)
        {
            Register = register;
        }

        public override string Emit()
        {
            return $"fstp {Register}";
        }
    }

    public class Fld : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public Fld(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fld {Source}";
        }
    }

    public class Fild : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public Fild(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fild {Source}";
        }
    }

    public class Fistp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public Fistp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fistp {Source}";
        }
    }

    public class FAddp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FAddp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"faddp {Source}";
        }
    }

    public class FiAddp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiAddp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fiaddp {Source}";
        }
    }

    public class FSubp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FSubp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fsubp {Source}";
        }
    }

    public class FiSubp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiSubp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fisubp {Source}";
        }
    }

    public class FMulp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FMulp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fmulp {Source}";
        }
    }

    public class FiMulp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiMulp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fimulp {Source}";
        }
    }

    public class FDivp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FDivp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fdivp {Source}";
        }
    }
    public class FiDivp : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiDivp(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fidivp {Source}";
        }
    }

    public class FComip : X86Instruction
    {

        public FComip()
        {
        }

        public override string Emit()
        {
            return $"fcomip";
        }
    }

}
