using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using static PlatinumC.Compiler.TargetX86.Instructions.Mov_SymbolOffset_Register;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace PlatinumC.Compiler.TargetX86.Instructions
{
    public static class X86Instructions
    {
        public static Cdq Cdq() => new Cdq();
        public static Push_Register Push(X86Register register) => new Push_Register(register);
        public static Push_Offset Push(RegisterOffset offset) => new Push_Offset(offset);
        public static Push_Address Push(string address, bool isIndirect) => new Push_Address(address, isIndirect);
        public static Push_Immediate<int> Push(int immediateValue) => new Push_Immediate<int>(immediateValue);
        public static Push_SymbolOffset Push(SymbolOffset offset) => new Push_SymbolOffset(offset);

        public static Lea_Register_Offset Lea(X86Register destination, RegisterOffset source) => new Lea_Register_Offset(destination, source);
        public static Lea_Register_SymbolOffset Lea(X86Register destination, SymbolOffset source) => new Lea_Register_SymbolOffset(destination, source);

        public static Mov_Register_Offset Mov(X86Register destination, RegisterOffset source) => new Mov_Register_Offset(destination, source);
        public static Mov_Offset_Register Mov(RegisterOffset destination, X86Register source) => new Mov_Offset_Register(destination, source);
        public static Mov_Offset_Immediate Mov(RegisterOffset destination, int immediate) => new Mov_Offset_Immediate(destination, immediate);
        public static Mov_Register_Register Mov(X86Register destination, X86Register source) => new Mov_Register_Register(destination, source);
        public static Mov_Register_Immediate Mov(X86Register destination, int immediate) => new Mov_Register_Immediate(destination, immediate);

        public static Mov_SymbolOffset_Register Mov(SymbolOffset destination, X86Register source) => new Mov_SymbolOffset_Register(destination, source);
        public static Mov_SymbolOffset_Register__Byte Mov(SymbolOffset destination, X86ByteRegister source) => new Mov_SymbolOffset_Register__Byte(destination, source);
        public static Mov_SymbolOffset_Immediate Mov(SymbolOffset destination, int immediateValue) => new Mov_SymbolOffset_Immediate(destination, immediateValue);

        public static Mov_Offset_Register__Byte Mov(RegisterOffset destination, X86ByteRegister source) => new Mov_Offset_Register__Byte(destination, source);
        public static Movsx_Register_Offset Movsx(X86Register destination, RegisterOffset_Byte source) => new Movsx_Register_Offset(destination, source);
        public static Movsx_Register_SymbolOffset__Byte Movsx(X86Register destination, SymbolOffset_Byte source) => new Movsx_Register_SymbolOffset__Byte(destination, source);

        public static Sub_Register_Immediate Sub(X86Register destination, int valueToSubtract) => new Sub_Register_Immediate(destination, valueToSubtract);
        public static Sub_Register_Register Sub(X86Register destination, X86Register source) => new Sub_Register_Register(destination, source);

        public static Add_Register_Immediate Add(X86Register destination, int value) => new Add_Register_Immediate(destination, value);
        public static Add_Register_Register Add(X86Register destination, X86Register source) => new Add_Register_Register(destination, source);


        public static And_Register_Register And(X86Register destination, X86Register source) => new And_Register_Register(destination, source);
        public static Or_Register_Register Or(X86Register destination, X86Register source) => new Or_Register_Register(destination, source);
        public static Xor_Register_Register Xor(X86Register destination, X86Register source) => new Xor_Register_Register(destination, source);


        public static Pop_Register Pop(X86Register destination) => new Pop_Register(destination);

        public static Neg_Offset Neg(RegisterOffset destination) => new Neg_Offset(destination);
        public static Not_Offset Not(RegisterOffset destination) => new Not_Offset(destination);

        public static Inc_Register Inc(X86Register destination) => new Inc_Register(destination);
        public static Dec_Register Dec(X86Register destination) => new Dec_Register(destination);

        public static IDiv_Offset IDiv(RegisterOffset divisor) => new IDiv_Offset(divisor);
        public static IMul_Register_Register IMul(X86Register destination, X86Register source) => new IMul_Register_Register(destination, source);
        public static IMul_Register_Immediate IMul(X86Register destination, int immediate) => new IMul_Register_Immediate(destination, immediate);
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

        public static Test_Register_Register Test(X86Register operand1, X86Register operand2) => new Test_Register_Register(operand1, operand2);
        public static Test_Register_Offset Test(X86Register operand1, RegisterOffset operand2) => new Test_Register_Offset(operand1, operand2);
        public static Cmp_Register_Register Cmp(X86Register operand1, X86Register operand2) => new Cmp_Register_Register(operand1, operand2);
        public static Cmp_Register_Immediate Cmp(X86Register operand1, int operand2) => new Cmp_Register_Immediate(operand1, operand2);
        public static Cmp_Byte_Byte Cmp(X86ByteRegister operand1, X86ByteRegister operand2) => new Cmp_Byte_Byte(operand1, operand2);

        public static Call Call(string callee, bool isIndirect) => new Call(callee, isIndirect);
        public static Label Label(string text) => new Label(text);
        public static Ret Ret() => new Ret();
        public static Ret_Immediate Ret(int immediate) => new Ret_Immediate(immediate);

        public static Fstp_Offset Fstp(RegisterOffset destination) => new Fstp_Offset(destination);
        public static Fld_Offset Fld(RegisterOffset source) => new Fld_Offset(source);

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
