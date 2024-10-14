using CliParser;
using PlatinumC.Compiler;
using System.Net.Sockets;

namespace PlatinumC.Services
{
    [Entry("c")]
    internal class StartupService
    {
        [Command]
        public void Compile(string inputPath, string? outputPath = null, string? assemblyPath = null)
        {
            var compilationOptions = new CompilationOptions()
            {
                InputPath = inputPath,
                AssemblyPath = assemblyPath ?? Path.ChangeExtension(inputPath, ".asm"),
                OutputPath = outputPath ?? Path.ChangeExtension(inputPath, ".exe"), 
                EnableOptimizations = true
            };
            var compiler = new X86ProgramCompiler();

            var result = compiler.EmitBinary(compilationOptions);

            if (result != null) Console.WriteLine(result);
        }

    }
}
