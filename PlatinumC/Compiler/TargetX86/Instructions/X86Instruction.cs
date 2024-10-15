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
        public static RegisterOffset Create(X86Register register, int offset, bool isIndirect) => new RegisterOffset(register, offset, isIndirect);
    }
    public class RegisterOffset
    {
        public X86Register Register { get; set; }
        public int Offset { get; set; }
        public bool IsIndirect { get; set; }
        public RegisterOffset(X86Register register, int offset, bool isIndirect)
        {
            Register = register;
            Offset = offset;
            IsIndirect = isIndirect;
        }

        public bool IsPlainRegister => Offset == 0 && !IsIndirect;

        public override string ToString()
        {
            var repr = Offset == 0 ? Register.ToString() : $"{Register} {(Offset > 0 ? "+" : "-")} {Math.Abs(Offset)}";
            if (IsIndirect) return $"dword [{repr}]";
            return repr;
        }

        public override bool Equals(object? obj)
        {
            if (obj is RegisterOffset offset)
            {
                return Offset == offset.Offset && IsIndirect == offset.IsIndirect && Register == offset.Register;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Register.GetHashCode();
        }
    }
}
