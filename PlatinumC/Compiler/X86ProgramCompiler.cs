using PlatinumC.CodeGenerator.Fasm;
using PlatinumC.Compiler.TargetX86;
using PlatinumC.Compiler.TargetX86.Instructions;
using PlatinumC.Extensions;
using PlatinumC.Optimizer;
using PlatinumC.Parser;
using PlatinumC.Resolver;
using PlatinumC.Shared;
using System.Text;
using TokenizerCore.Interfaces;


namespace PlatinumC.Compiler
{
    public class X86CompilationContext
    {

        public class ImportLibrary
        {
            public class ImportedFunction
            {
                public IToken Symbol { get; set; }
                public IToken FunctionIdentifier { get; set; }
                public ImportedFunction(IToken symbol, IToken functionIdentifier)
                {
                    Symbol = symbol;
                    FunctionIdentifier = functionIdentifier;
                }
            }
            public IToken LibraryPath { get; set; }
            public IToken LibraryAlias { get; set; }
            public List<ImportedFunction> ImportedFunctions { get; set; } = new();
            public ImportLibrary(IToken libraryPath, IToken libraryAlias)
            {
                LibraryPath = libraryPath;
                LibraryAlias = libraryAlias;
            }

            public void AddImportedFunction(IToken functionIdentifier, IToken symbol)
            {
                if (ImportedFunctions.Any(x => x.FunctionIdentifier.Lexeme == functionIdentifier.Lexeme))
                    throw new Exception($"symbol with name {functionIdentifier} is already imported");
                ImportedFunctions.Add(new(symbol, functionIdentifier));
            }
        }

        public class StringData
        {
            public string Label { get; set; }
            public string Value { get; set; }
            public StringData(string label, string value)
            {
                Label = label;
                Value = value;
            }

            public string Emit(int indentLevel)
            {
                return $"{Label} db {EscapeString(Value)},0".Indent(indentLevel);
            }

            private static string EscapeString(string value)
            {
                if (value.Length == 0) return "0";
                var bytes = Encoding.UTF8.GetBytes(value);
                var strBytes = BitConverter.ToString(bytes);
                return $"0x{strBytes.Replace("-", ",0x")}";
            }
        }
        public CompilationOptions CompilationOptions { get; private set; }

        public X86CompilationContext(CompilationOptions compilationOptions)
        {
            CompilationOptions = compilationOptions;
        }

        private X86Function? _currentFunctionTarget;
        private int _stringUniqueIndex = 0;

        private int _loopUniqueIndex = 0;

        private Stack<(string continueLabel, string breakLabel)> _loopLabels = new Stack<(string continueLabel, string breakLabel)>();
        private List<StringData> _stringData = new();
        public List<StringData> StaticStringData => _stringData;
        public int SizeOfPtr => 4;

        public List<X86Function> FunctionData { get; private set; } = new();
        public List<ImportLibrary> ImportLibraries { get; private set; } = new();
        public List<(string functionIdentifier, string exportedSymbol)> ExportedFunctions => FunctionData.Where(x => x.IsExported).Select(x => (x.OriginalDeclaration.GetDecoratedFunctionIdentifier(), x.ExportedSymbol.Lexeme)).ToList();
        public void AddImportedFunction(TypedImportedFunctionDeclaration importedFunction)
        {
            var foundLibrary = ImportLibraries.Find(x => x.LibraryAlias.Lexeme == importedFunction.LibraryAlias.Lexeme);
            if (foundLibrary == null) throw new Exception($"import library with alias {importedFunction.LibraryAlias} is not defined");
            foundLibrary.AddImportedFunction(importedFunction.FunctionIdentifier, importedFunction.FunctionSymbol);
        } 

        public void AddImportLibrary(TypedImportLibraryDeclaration library)
        {
            var foundLibrary = ImportLibraries.Find(x => x.LibraryAlias.Lexeme == library.LibraryAlias.Lexeme);
            if (foundLibrary != null) throw new Exception($"import library with alias {library.LibraryAlias} is already defined");
            ImportLibraries.Add(new(library.LibraryPath, library.LibraryAlias));
        }


        public void AddInstruction(X86Instruction x86Instruction)
        {
            if (_currentFunctionTarget == null) throw new InvalidOperationException();
            _currentFunctionTarget.AddInstruction(x86Instruction);  
        }

        public string AddStringData(string value)
        {
            var label = CreateUniqueStringLabel();
            _stringData.Add(new(label, value));
            return label;
        }

        public RegisterOffset GetIdentifierOffset(IToken identifier)
        {
            var foundParameterIndex = CurrentFunction.Parameters.FindIndex(x => x.ParameterName.Lexeme == identifier.Lexeme);
            if (foundParameterIndex != -1) return new RegisterOffset(X86Register.ebp, 8 + (foundParameterIndex * 4), true);
            var foundLocalVariableIndex = CurrentFunction.LocalVariables.FindIndex(x => x.Identifier.Lexeme == identifier.Lexeme);
            if (foundLocalVariableIndex != -1) return new RegisterOffset(X86Register.ebp, -4 - (foundLocalVariableIndex * 4), true);
            throw new Exception($"local variable {identifier} does not exist");
        }

        public void EnterFunction(TypedFunctionDeclaration typedFunctionDeclaration)
        {
            if (_currentFunctionTarget != null) throw new InvalidOperationException();
            _currentFunctionTarget = new X86Function(typedFunctionDeclaration);
        }

        public void ExitFunction()
        {
            if (_currentFunctionTarget == null) throw new InvalidOperationException();
            FunctionData.Add(_currentFunctionTarget);
            _currentFunctionTarget = null;

        }

        public X86Function CurrentFunction => _currentFunctionTarget ?? throw new InvalidOperationException();

        private string CreateUniqueStringLabel()
        {
            return $"!str_{_stringUniqueIndex++}";
        }

        private string CreateLoopLabel()
        {
            return $"!loop_{_loopUniqueIndex++}";
        }

        public string CreateLabel()
        {
            return $"!label_{_stringUniqueIndex++}";
        }

        public string EnterLoop(string continueLabel)
        {
            var breakLabel = CreateLoopLabel();
            _loopLabels.Push((continueLabel, breakLabel));
            return breakLabel;
        }

        public void ExitLoop()
        {
            _loopLabels.Pop();
        }


        public string GetLoopBreakLabel()
        {
            if (_loopLabels.Count == 0) throw new InvalidOperationException();
            return _loopLabels.Peek().breakLabel;
        }

        public string GetLoopContinueLabel()
        {
            if ( _loopLabels.Count == 0 ) throw new InvalidOperationException();
            return _loopLabels.Peek().continueLabel;
        }

    }
    public class X86ProgramCompiler
    {
        private readonly ProgramParser _parser = new();
        private readonly TypeResolver _resolver = new();
        private readonly X86Optimizer _optimizer = new(); 
        public string? EmitBinary(CompilationOptions compilationOptions)
        {
            var result = Compile(compilationOptions);
            return X86CodeGenerator.Generate(result);
        }

        public CompilationResult Compile(CompilationOptions options)
        {
            var parserResult = _parser.ParseFile(options.InputPath, out var errors);
            if (errors.Any()) throw new Exception();
            var resolverResult = _resolver.ResolveTypes(parserResult);
            return Compile(resolverResult, options);
        }

        public CompilationResult Compile(ResolverResult resolverResult, CompilationOptions options)
        {
            var context = new X86CompilationContext(options);
            resolverResult.TypedDeclarations.ForEach(x => x.Visit(context));
            var result = new CompilationResult(context);
            if (options.EnableOptimizations)
                _optimizer.Optimize(result);
            return result;
        }

        
    }
}
