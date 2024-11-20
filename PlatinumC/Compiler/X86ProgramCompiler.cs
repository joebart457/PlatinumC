using PlatinumC.CodeGenerator.Fasm;
using PlatinumC.Optimizer;
using PlatinumC.Parser;
using PlatinumC.Resolver;


namespace PlatinumC.Compiler
{
    public class X86ProgramCompiler
    {
        private readonly ProgramParser _parser = new();
        private readonly TypeResolver _resolver = new();
        private readonly X86AssemblyOptimizer _optimizer = new(); 
        public string? EmitBinary(CompilationOptions compilationOptions)
        {
            var result = Compile(compilationOptions);
            return X86CodeGenerator.Generate(result);
        }

        public CompilationResult Compile(CompilationOptions options)
        {
            var parserResult = _parser.ParseFile(options.InputPath, out var errors);
            if (errors.Any()) throw new AggregateException(errors);
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
