using PlatinumC.Compiler.TargetX86.Instructions;
using PlatinumC.Extensions;
using PlatinumC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TokenizerCore.Interfaces;
using static PlatinumC.Shared.TypedFunctionDeclaration;

namespace PlatinumC.Compiler.TargetX86
{
    public class X86Function
    {
        public TypedFunctionDeclaration OriginalDeclaration { get; set; }
        public List<TypedVariableDeclaration> LocalVariables => OriginalDeclaration.LocalVariableDeclarations;
        public List<X86Instruction> Instructions { get; set; } = new();
        public CallingConvention CallingConvention => OriginalDeclaration.CallingConvention;    
        public List<TypedParameterDeclaration> Parameters => OriginalDeclaration.Parameters;
        public bool IsExported => OriginalDeclaration.IsExport;
        public IToken ExportedSymbol => OriginalDeclaration.ExportedSymbol;
        public X86Function(TypedFunctionDeclaration originalDeclaration)
        {
            OriginalDeclaration = originalDeclaration;
            
        }

        public void AddInstruction(X86Instruction instruction)
        {
            Instructions.Add(instruction);
        }

        public string Emit(int indentLevel)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"{OriginalDeclaration.FunctionIdentifier.Lexeme}:".Indent(indentLevel + 1));
            foreach(var instruction in Instructions)
            {
                sb.AppendLine(instruction.Emit().Indent(indentLevel + 2));
            }
            return sb.ToString();
        }
    }
}
