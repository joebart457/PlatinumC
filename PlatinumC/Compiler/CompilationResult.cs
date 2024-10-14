using PlatinumC.Compiler.TargetX86;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlatinumC.Compiler.X86CompilationContext;

namespace PlatinumC.Compiler
{
    public class CompilationResult
    {
        public CompilationOptions CompilationOptions { get; private set; }
        public List<X86Function> FunctionData { get; private set; } 
        public List<ImportLibrary> ImportLibraries { get; private set; } 
        public List<(string functionIdentifier, string exportedSymbol)> ExportedFunctions { get; private set; }
        public List<StringData> StaticStringData { get; private set; }

        public CompilationResult(X86CompilationContext context)
        {
            CompilationOptions = context.CompilationOptions;
            FunctionData = context.FunctionData;
            ImportLibraries = context.ImportLibraries;
            ExportedFunctions = context.ExportedFunctions;
            StaticStringData = context.StaticStringData;
        }
    }
}
