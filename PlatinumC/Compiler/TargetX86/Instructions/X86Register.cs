using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public enum X86Register
    {
        eax,
        ebx,
        ecx,
        edx,
        esi,
        edi,
        esp,
        ebp,
    }

    public enum X86ByteRegister
    {
        al,
        bl,
    }

    public static class X86ByteRegisterExtensions
    {
        public static X86Register ToFullRegister(this X86ByteRegister byteRegister)
        {
            if (byteRegister == X86ByteRegister.al) return X86Register.eax;
            if (byteRegister == X86ByteRegister.bl) return X86Register.ebx;
            throw new InvalidOperationException();
        }
    }
        
}
