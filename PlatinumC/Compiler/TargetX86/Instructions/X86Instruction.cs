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
        public override string ToString()
        {
            return Emit();
        }
    }
    public interface IOffset
    {
        public int Offset { get; set; }
    }

    public class Offset
    {
        public static RegisterOffset Create(X86Register register, int offset) => new RegisterOffset(register, offset);
        public static RegisterOffset_Byte CreateByteOffset(X86Register register, int offset) => new RegisterOffset_Byte(register, offset);
        public static SymbolOffset CreateSymbolOffset(string symbol, int offset) => new SymbolOffset(symbol, offset);
        public static SymbolOffset_Byte CreateSymbolByteOffset(string symbol, int offset) => new SymbolOffset_Byte(symbol, offset);
    }
    public class RegisterOffset: IOffset
    {
        public X86Register Register { get; set; }
        public int Offset { get; set; }
        public RegisterOffset(X86Register register, int offset)
        {
            Register = register;
            Offset = offset;
        }

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

    public class SymbolOffset : IOffset
    {
        public string Symbol { get; set; }
        public int Offset { get; set; }
        public SymbolOffset(string symbol, int offset)
        {
            Symbol = symbol;
            Offset = offset;
        }

        public override string ToString()
        {
            var repr = Offset == 0 ? Symbol : $"{Symbol} {(Offset > 0 ? "+" : "-")} {Math.Abs(Offset)}";
            return $"dword [{repr}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is SymbolOffset offset)
            {
                return Offset == offset.Offset && Symbol == offset.Symbol;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }
    }

    public class SymbolOffset_Byte : IOffset
    {
        public string Symbol { get; set; }
        public int Offset { get; set; }
        public SymbolOffset_Byte(string symbol, int offset)
        {
            Symbol = symbol;
            Offset = offset;
        }

        public override string ToString()
        {
            var repr = Offset == 0 ? Symbol : $"{Symbol} {(Offset > 0 ? "+" : "-")} {Math.Abs(Offset)}";
            return $"byte [{repr}]";
        }

        public override bool Equals(object? obj)
        {
            if (obj is SymbolOffset offset)
            {
                return Offset == offset.Offset && Symbol == offset.Symbol;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Symbol.GetHashCode();
        }
    }

    public class RegisterOffset_Byte : IOffset
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
