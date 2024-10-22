using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Fstp_Offset : X86Instruction
    {
        public RegisterOffset Destination { get; set; }

        public Fstp_Offset(RegisterOffset destination)
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

    public class Fld_Offset : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public Fld_Offset(RegisterOffset source)
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

    public class FAdd : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FAdd(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fadd {Source}";
        }
    }

    public class FiAdd : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiAdd(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fiadd {Source}";
        }
    }

    public class FSub : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FSub(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fsub {Source}";
        }
    }

    public class FiSub : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiSub(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fisub {Source}";
        }
    }

    public class FMul : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FMul(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fmul {Source}";
        }
    }

    public class FiMul : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiMul(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fimul {Source}";
        }
    }

    public class FDiv : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FDiv(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fdiv {Source}";
        }
    }
    public class FiDiv : X86Instruction
    {
        public RegisterOffset Source { get; set; }

        public FiDiv(RegisterOffset source)
        {
            Source = source;
        }

        public override string Emit()
        {
            return $"fidiv {Source}";
        }
    }

    public class FAddp : X86Instruction
    {

        public override string Emit()
        {
            return $"faddp";
        }
    }

    public class FiAddp : X86Instruction
    {
        public override string Emit()
        {
            return $"fiaddp";
        }
    }

    public class FSubp : X86Instruction
    {

        public override string Emit()
        {
            return $"fsubp";
        }
    }

    public class FiSubp : X86Instruction
    {

        public override string Emit()
        {
            return $"fisubp";
        }
    }

    public class FMulp : X86Instruction
    {
        public override string Emit()
        {
            return $"fmulp";
        }
    }

    public class FiMulp : X86Instruction
    {

        public override string Emit()
        {
            return $"fimulp";
        }
    }

    public class FDivp : X86Instruction
    {

        public override string Emit()
        {
            return $"fdivp";
        }
    }
    public class FiDivp : X86Instruction
    {
        public override string Emit()
        {
            return $"fidivp";
        }
    }

    public class FComip : X86Instruction
    {
        public X87Register Operand { get; set; }
        public FComip(X87Register operand)
        {
            Operand = operand;
        }

        public override string Emit()
        {
            return $"fcomip {Operand}";
        }
    }

}
