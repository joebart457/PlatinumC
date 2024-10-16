using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public static class X86Instructions
    {
        public static Cdq Cdq() => new Cdq();
        public static Push Push(X86Register register, bool isIndirect) => new Push(register, isIndirect);
        public static Push_Offset Push(RegisterOffset offset) => new Push_Offset(offset);
        public static Push_Address Push(string address, bool isIndirect) => new Push_Address(address, isIndirect);
        public static Push_Immediate<int> Push(int immediateValue) => new Push_Immediate<int>(immediateValue);
        public static Push_Immediate<float> Push(float immediateValue) => new Push_Immediate<float>(immediateValue);

        public static Lea Lea(X86Register destination, RegisterOffset source) => new Lea(destination, source);

        public static Mov_Register_Offset Mov(X86Register destination, RegisterOffset source) => new Mov_Register_Offset(destination, source);
        public static Mov_Offset_Register Mov(RegisterOffset destination, X86Register source) => new Mov_Offset_Register(destination, source);
        public static Mov_Offset_Immediate Mov(RegisterOffset destination, int immediate) => new Mov_Offset_Immediate(destination, immediate);
        public static Mov_Register_Register Mov(X86Register destination, X86Register source) => new Mov_Register_Register(destination, source);
        public static Mov_Register_Immediate Mov(X86Register destination, int immediate) => new Mov_Register_Immediate(destination, immediate);

        public static Sub Sub(X86Register destination, int valueToSubtract) => new Sub(destination, valueToSubtract);
        public static Sub_Register_Register Sub(X86Register destination, X86Register source) => new Sub_Register_Register(destination, source);

        public static Add_Register_Immediate Add(X86Register destination, int value) => new Add_Register_Immediate(destination, value);
        public static Add_Register_Register Add(X86Register destination, X86Register source) => new Add_Register_Register(destination, source);

        public static Pop_Register Pop(X86Register destination) => new Pop_Register(destination);


        public static IDiv IDiv(RegisterOffset divisor) => new IDiv(divisor);
        public static IMul IMul(X86Register destination, X86Register source) => new IMul(destination, source);
        public static IMul_Immediate IMul(X86Register destination, int immediate) => new IMul_Immediate(destination, immediate);
        public static Add_Register_Offset Add(X86Register destination, RegisterOffset source) => new Add_Register_Offset(destination, source);
        

        public static Jmp Jmp(string label) => new Jmp(label);
        public static JmpGt JmpGt(string label) => new JmpGt(label);
        public static JmpGte JmpGte(string label) => new JmpGte(label);
        public static JmpLt JmpLt(string label) => new JmpLt(label);
        public static JmpLte JmpLte(string label) => new JmpLte(label);
        public static JmpEq JmpEq(string label) => new JmpEq(label);
        public static JmpNeq JmpNeq(string label) => new JmpNeq(label);
        public static Jz Jz(string label) => new Jz(label);
        public static Jnz Jnz(string label) => new Jnz(label);
        public static Js Js(string label) => new Js(label);
        public static Jns Jns(string label) => new Jns(label);
        public static Ja Ja(string label) => new Ja(label);
        public static Jae Jae(string label) => new Jae(label);
        public static Jb Jb(string label) => new Jb(label);
        public static Jbe Jbe(string label) => new Jbe(label);

        public static Test Test(X86Register operand1, X86Register operand2) => new Test(operand1, operand2);
        public static Test_Offset Test(X86Register operand1, RegisterOffset operand2) => new Test_Offset(operand1, operand2);
        public static Cmp Cmp(X86Register operand1, X86Register operand2) => new Cmp(operand1, operand2);


        public static Call Call(string callee, bool isIndirect) => new Call(callee, isIndirect);
        public static Label Label(string text) => new Label(text);
        public static Ret Ret() => new Ret();
        public static Ret_Immediate Ret(int immediate) => new Ret_Immediate(immediate);



        public static Fstp Fstp(RegisterOffset destination) => new Fstp(destination);
        public static Fstp_Register Fstp(X87Register register) => new Fstp_Register(register);
        public static Fld Fld(RegisterOffset source) => new Fld(source);
        public static Fild Fild(RegisterOffset source) => new Fild(source);
        public static FAddp FAddp(RegisterOffset source) => new FAddp(source);
        public static FiAddp FiAddp(RegisterOffset source) => new FiAddp(source);
        public static FSubp FSubp(RegisterOffset source) => new FSubp(source);
        public static FiSubp FiSubp(RegisterOffset source) => new FiSubp(source);
        public static FMulp FMulp(RegisterOffset source) => new FMulp(source);
        public static FiMulp FiMulp(RegisterOffset source) => new FiMulp(source);
        public static FDivp FDivp(RegisterOffset source) => new FDivp(source);
        public static FiDivp FiDivp(RegisterOffset source) => new FiDivp(source);
        public static FComip FComip() => new FComip();

    }
}
