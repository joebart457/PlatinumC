
namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public class Movss_Offset_Register: X86Instruction
    {
        public RegisterOffset Destination { get; set; }
        public XmmRegister Source { get; set; }
        public Movss_Offset_Register(RegisterOffset destination, XmmRegister source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"movss {Destination}, {Source}";
        }
    }

    public class Movss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Movss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"movss {Destination}, {Source}";
        }
    }


    public class Comiss_Register_Offset : X86Instruction
    {
        public XmmRegister Operand1 { get; set; }
        public RegisterOffset Operand2 { get; set; }
        public Comiss_Register_Offset(XmmRegister operand1, RegisterOffset operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"comiss {Operand1}, {Operand2}";
        }
    }

    public class Comiss_Register_Register : X86Instruction
    {
        public XmmRegister Operand1 { get; set; }
        public XmmRegister Operand2 { get; set; }
        public Comiss_Register_Register(XmmRegister operand1, XmmRegister operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"comiss {Operand1}, {Operand2}";
        }
    }

    public class Ucomiss_Register_Register : X86Instruction
    {
        public XmmRegister Operand1 { get; set; }
        public XmmRegister Operand2 { get; set; }
        public Ucomiss_Register_Register(XmmRegister operand1, XmmRegister operand2)
        {
            Operand1 = operand1;
            Operand2 = operand2;
        }

        public override string Emit()
        {
            return $"ucomiss {Operand1}, {Operand2}";
        }
    }

    public class Addss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Addss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"addss {Destination}, {Source}";
        }
    }

    public class Subss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Subss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"subss {Destination}, {Source}";
        }
    }

    public class Divss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Divss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"divss {Destination}, {Source}";
        }
    }

    public class Mulss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Mulss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"divss {Destination}, {Source}";
        }
    }

    public class Cvtsi2ss_Register_Offset : X86Instruction
    {
        public XmmRegister Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Cvtsi2ss_Register_Offset(XmmRegister destination, RegisterOffset source)
        {
            Destination = destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"cvtsi2ss {Destination}, {Source}";
        }
    }

    public class Cvtss2si_Register_Offset : X86Instruction
    {
        public X86Register Destination { get; set; }
        public RegisterOffset Source { get; set; }
        public Cvtss2si_Register_Offset(X86Register destination, RegisterOffset source)
        {
            Destination=destination;
            Source = source;
        }

        public override string Emit()
        {
            return $"cvtss2si {Destination}, {Source}";
        }
    }
}


