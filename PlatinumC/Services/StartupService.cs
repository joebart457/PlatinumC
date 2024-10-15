using CliParser;
using Logger;
using PlatinumC.Compiler;

namespace PlatinumC.Services
{
    [Entry("PlatinumC.exe")]
    internal class StartupService
    {
        [Command]
        public void Compile(
            [Option("inputPath", "i", "the path of the source file to be compiled")] string inputPath,
            [Option("outputPath", "o", "the desired path of the resulting binary")] string? outputPath = null,
            [Option("assemblyPath", "a", "the path to save the generated intermediate assembly. Option can be ignored if only final binary is desired.")] string? assemblyPath = null,
            [Option("target", "t", "the target binary format. Valid options are exe or dll.")] string? target = null,
            [Option("enableOptimizations", "x", "whether or not to allow the compiler to optimize the generated assembly")] bool enableOptimizations = true,
            [Option("numberOfPasses", "n", "number of optimization passes to make. Ignored if enableOptimizations is false")] int numberOfPasses = 3)
        {
            var outputTarget = OutputTarget.Exe;
            if (!string.IsNullOrWhiteSpace(target))
            {
                if (!Enum.TryParse(target, true, out outputTarget))
                    CliLogger.LogError($"invalid value for option -t target. Value must be one of {string.Join(", ", Enum.GetNames<OutputTarget>())}");
            }

            var compilationOptions = new CompilationOptions()
            {
                InputPath = inputPath,
                AssemblyPath = assemblyPath ?? "",
                OutputPath = outputPath ?? "", 
                OutputTarget = outputTarget,
                EnableOptimizations = enableOptimizations,
                OptimizationPasses = numberOfPasses,
            };
            var compiler = new X86ProgramCompiler();

            var result = compiler.EmitBinary(compilationOptions);

            if (result != null) Console.WriteLine(result);
        }

    }
}
