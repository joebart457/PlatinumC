using CliParser;
using Logger;
using PlatinumC.CodeGenerator.Fasm;
using PlatinumC.Compiler;

namespace PlatinumC.Services
{
    [Entry("PlatinumC.exe")]
    public class StartupService
    {
        [Command]
        public int Compile(
            [Option("inputPath", "i", "the path of the source file to be compiled")] string inputPath,
            [Option("outputPath", "o", "the desired path of the resulting binary")] string? outputPath = null,
            [Option("assemblyPath", "a", "the path to save the generated intermediate assembly. Option can be ignored if only final binary is desired.")] string? assemblyPath = null,
            [Option("target", "t", "the target binary format. Valid options are exe or dll.")] string? target = null,
            [Option("enableOptimizations", "x", "whether or not to allow the compiler to optimize the generated assembly")] bool enableOptimizations = true,
            [Option("numberOfPasses", "n", "number of optimization passes to make. Ignored if enableOptimizations is false")] int numberOfPasses = 3,
            [Option("sourceComments", "sc", "if enabled, generated assembly will contain source comments")] bool sourceComments = false,
            [Option("enableInMemoryCompilation", "m", "if enabled, assembly will happen in memory, and no assembly file will be generated")] bool enableInMemoryCompilation = false,
            [Option("compilationMemoryBuffer", "mb", "size of memory in bytes the compiler will use for assembly")] int compilationMemoryBuffer = 100000,
            [Option("assemblyPasses", "na", "number of passes the assembler is allowed to use when attempting to generate final binary")] int assemblyPasses = 100)
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
                SourceComments = sourceComments,
                AssemblerOptions = new()
                {
                    EnableInMemoryAssembly = enableInMemoryCompilation,
                    MemorySize = compilationMemoryBuffer,
                    PassesLimit = assemblyPasses,
                }
            };
            var compiler = new X86ProgramCompiler();

            var result = compiler.EmitBinary(compilationOptions);

            if (result != null)
            {
                CliLogger.LogError(result);
                return -1;
            }
            return 0;
        }

        [Command("fasm")]
        public int RunFasm(      
           [Option("assemblyPath", "a", "the path of the input assembly code.")] string? assemblyPath = null,
           [Option("compilationMemoryBuffer", "mb", "size of memory in bytes the compiler will use for assembly")] int compilationMemoryBuffer = 100000,
           [Option("assemblyPasses", "na", "number of passes the assembler is allowed to use when attempting to generate final binary")] int assemblyPasses = 100)
        {           

            var compilationOptions = new CompilationOptions()
            {
                AssemblyPath = assemblyPath ?? "",
                AssemblerOptions = new()
                {
                    EnableInMemoryAssembly = true,
                    MemorySize = compilationMemoryBuffer,
                    PassesLimit = assemblyPasses,
                }
            };

            var result = FasmDllService.RunFasm(compilationOptions);

            if (result != null)
            {
                CliLogger.LogError(result);
                return -1;
            }
            return 0;
        }

    }
}
