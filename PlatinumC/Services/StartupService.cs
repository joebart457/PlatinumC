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
           [Option("assemblyPath", "a", "the path of the input assembly code.")] string assemblyPath,
           [Option("outputPath", "o", "the desired path of the resulting binary")] string outputPath,
           [Option("compilationMemoryBuffer", "mb", "size of memory in bytes the compiler will use for assembly")] int compilationMemoryBuffer = 100000,
           [Option("assemblyPasses", "na", "number of passes the assembler is allowed to use when attempting to generate final binary")] int assemblyPasses = 100,
           [Option("copyright", "c", "show copyright")] bool showCopyright = false)
        {           

            var compilationOptions = new CompilationOptions()
            {
                AssemblyPath = assemblyPath,
                OutputPath = outputPath,
                AssemblerOptions = new()
                {
                    EnableInMemoryAssembly = true,
                    MemorySize = compilationMemoryBuffer,
                    PassesLimit = assemblyPasses,
                }
            };
            if (showCopyright) CliLogger.LogInfo(FasmCopyright);
            var result = FasmDllService.RunFasm(compilationOptions);

            if (result != null)
            {
                CliLogger.LogError(result);
                return -1;
            }
            return 0;
        }

        private const string FasmCopyright = "flat assembler  version 1.73\r\nCopyright (c) 1999-2024, Tomasz Grysztar.\r\nAll rights reserved.\r\n\r\nThis program is free for commercial and non-commercial use as long as\r\nthe following conditions are adhered to.\r\n\r\nCopyright remains Tomasz Grysztar, and as such any Copyright notices\r\nin the code are not to be removed.\r\n\r\nRedistribution and use in source and binary forms, with or without\r\nmodification, are permitted provided that the following conditions are\r\nmet:\r\n\r\n1. Redistributions of source code must retain the above copyright notice,\r\n   this list of conditions and the following disclaimer.\r\n2. Redistributions in binary form must reproduce the above copyright\r\n   notice, this list of conditions and the following disclaimer in the\r\n   documentation and/or other materials provided with the distribution.\r\n\r\nTHIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS\r\n\"AS IS\" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED\r\nTO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A\r\nPARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE REGENTS OR\r\nCONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,\r\nEXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,\r\nPROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR\r\nPROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF\r\nLIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING\r\nNEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS\r\nSOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.\r\n\r\nThe licence and distribution terms for any publically available\r\nversion or derivative of this code cannot be changed. i.e. this code\r\ncannot simply be copied and put under another distribution licence\r\n(including the GNU Public Licence).\r\n";

    }
}
