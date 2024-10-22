using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public abstract class X86Instruction
    {
        public abstract string Emit();
    }

    public class Offset
    {
        public static RegisterOffset Create(X86Register register, int offset) => new RegisterOffset(register, offset);
        public static RegisterOffset_Byte CreateByteOffset(X86Register register, int offset) => new RegisterOffset_Byte(register, offset);
    }
    public class RegisterOffset
    {
        public X86Register Register { get; set; }
        public int Offset { get; set; }
        public bool IsIndirect { get; set; }
        public RegisterOffset(X86Register register, int offset)
        {
            Register = register;
            Offset = offset;
        }

        public bool IsPlainRegister => Offset == 0 && !IsIndirect;

        public override string ToString()
        {
            var repr = Offset == 0 ? Register.ToString() : $"{Register} {(Offset > 0 ? "+" : "-")} {Math.Abs(Offset)}";
            return $"dword [{repr}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is RegisterOffset offset)
            {
                return Offset == offset.Offset && Register == offset.Register;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Register.GetHashCode();
        }
    }

    public class RegisterOffset_Byte
    {
        public X86Register Register { get; set; }
        public int Offset { get; set; }
        public RegisterOffset_Byte(X86Register register, int offset)
        {
            Register = register;
            Offset = offset;
        }


        public override string ToString()
        {
            var repr = Offset == 0 ? Register.ToString() : $"{Register} {(Offset > 0 ? "+" : "-")} {Math.Abs(Offset)}";
            return $"byte [{repr}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is RegisterOffset_Byte offset)
            {
                return Offset == offset.Offset && Register == offset.Register;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Register.GetHashCode();
        }
    }
}
