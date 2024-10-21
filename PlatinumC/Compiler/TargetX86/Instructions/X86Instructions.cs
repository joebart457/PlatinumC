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
        public static Mov_Register_Immediate__Byte Mov(X86ByteRegister destination, byte immediate) => new Mov_Register_Immediate__Byte(destination, immediate);

        public static Mov_Offset_Register__Byte Mov(RegisterOffset destination, X86ByteRegister source) => new Mov_Offset_Register__Byte(destination, source);
        public static Mov_Register_Offset__Byte Mov(X86ByteRegister destination, RegisterOffset source) => new Mov_Register_Offset__Byte(destination, source);
        public static Movsx Movsx(X86Register destination, X86ByteRegister source) => new Movsx(destination, source);
        public static Movzx Movzx(X86Register destination, X86ByteRegister source) => new Movzx(destination, source);

        public static Sub Sub(X86Register destination, int valueToSubtract) => new Sub(destination, valueToSubtract);
        public static Sub_Register_Register Sub(X86Register destination, X86Register source) => new Sub_Register_Register(destination, source);

        public static Add_Register_Immediate Add(X86Register destination, int value) => new Add_Register_Immediate(destination, value);
        public static Add_Register_Register Add(X86Register destination, X86Register source) => new Add_Register_Register(destination, source);


        public static And_Register_Register And(X86Register destination, X86Register source) => new And_Register_Register(destination, source);
        public static Or_Register_Register Or(X86Register destination, X86Register source) => new Or_Register_Register(destination, source);
        public static Xor_Register_Register Xor(X86Register destination, X86Register source) => new Xor_Register_Register(destination, source);
        public static And_Register_Register__Byte And(X86ByteRegister destination, X86ByteRegister source) => new And_Register_Register__Byte(destination, source);
        public static Or_Register_Register__Byte Or(X86ByteRegister destination, X86ByteRegister source) => new Or_Register_Register__Byte(destination, source);
        public static Xor_Register_Register__Byte Xor(X86ByteRegister destination, X86ByteRegister source) => new Xor_Register_Register__Byte(destination, source);


        public static Pop_Register Pop(X86Register destination) => new Pop_Register(destination);

        public static Neg Neg(RegisterOffset divisor) => new Neg(divisor);
        public static Not Not(RegisterOffset divisor) => new Not(divisor);

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
        public static Cmp_Register_Immediate Cmp(X86Register operand1, int operand2) => new Cmp_Register_Immediate(operand1, operand2);
        public static Cmp_Byte_Byte Cmp(X86ByteRegister operand1, X86ByteRegister operand2) => new Cmp_Byte_Byte(operand1, operand2);

        public static Call Call(string callee, bool isIndirect) => new Call(callee, isIndirect);
        public static Label Label(string text) => new Label(text);
        public static Ret Ret() => new Ret();
        public static Ret_Immediate Ret(int immediate) => new Ret_Immediate(immediate);



        public static Fstp Fstp(RegisterOffset destination) => new Fstp(destination);
        public static Fstp_Register Fstp(X87Register register) => new Fstp_Register(register);
        public static Fld Fld(RegisterOffset source) => new Fld(source);
        public static Fild Fild(RegisterOffset source) => new Fild(source);
        public static Fistp Fistp(RegisterOffset source) => new Fistp(source);
        public static FAdd FAdd(RegisterOffset source) => new FAdd(source);
        public static FiAdd FiAdd(RegisterOffset source) => new FiAdd(source);
        public static FSub FSub(RegisterOffset source) => new FSub(source);
        public static FiSub FiSub(RegisterOffset source) => new FiSub(source);
        public static FMul FMul(RegisterOffset source) => new FMul(source);
        public static FiMul FiMul(RegisterOffset source) => new FiMul(source);
        public static FDiv FDiv(RegisterOffset source) => new FDiv(source);
        public static FiDiv FiDiv(RegisterOffset source) => new FiDiv(source);
        public static FAddp FAddp() => new FAddp();
        public static FiAddp FiAddp() => new FiAddp();
        public static FSubp FSubp() => new FSubp();
        public static FiSubp FiSubp() => new FiSubp();
        public static FMulp FMulp() => new FMulp();
        public static FiMulp FiMulp() => new FiMulp();
        public static FDivp FDivp() => new FDivp();
        public static FiDivp FiDivp() => new FiDivp();
        public static FComip FComip() => new FComip(X87Register.st1);



        public static Movss_Offset_Register Movss(RegisterOffset destination, XmmRegister source) => new Movss_Offset_Register(destination, source);
        public static Movss_Register_Offset Movss(XmmRegister destination, RegisterOffset source) => new Movss_Register_Offset(destination, source);
        public static Comiss_Register_Offset Comiss(XmmRegister destination, RegisterOffset source) => new Comiss_Register_Offset(destination, source);
        public static Comiss_Register_Register Comiss(XmmRegister destination, XmmRegister source) => new Comiss_Register_Register(destination, source);
        public static Ucomiss_Register_Register Ucomiss(XmmRegister destination, XmmRegister source) => new Ucomiss_Register_Register(destination, source);
        public static Addss_Register_Offset Addss(XmmRegister destination, RegisterOffset source) => new Addss_Register_Offset(destination, source);
        public static Subss_Register_Offset Subss(XmmRegister destination, RegisterOffset source) => new Subss_Register_Offset(destination, source);
        public static Mulss_Register_Offset Mulss(XmmRegister destination, RegisterOffset source) => new Mulss_Register_Offset(destination, source);
        public static Divss_Register_Offset Divss(XmmRegister destination, RegisterOffset source) => new Divss_Register_Offset(destination, source);
        public static Cvtsi2ss_Register_Offset Cvtsi2ss(XmmRegister destination, RegisterOffset source) => new Cvtsi2ss_Register_Offset(destination, source);
        public static Cvtss2si_Register_Offset Cvtss2si(X86Register destination, RegisterOffset source) => new Cvtss2si_Register_Offset(destination, source);
    }
}
