using PlatinumC.Compiler;
using System.Runtime.InteropServices;
using System.Text;

namespace PlatinumC.CodeGenerator.Fasm
{
    internal static class FasmDllService
    {
        private struct FasmResult
        {
            public int Condition;
            public int OutputLengthOrErrorCode;
            public int OutputDataOrErrorLine;
        }
        private struct LineHeader
        {
            public IntPtr FilePath;
            public int LineNumber;
            public int FileOffsetOrMacroCallingLine;
            public int MacroLine;
        }

        private static class FasmState
        {
            public static int FASM_OK = 0;
            public static int FASM_ERROR = 2;
            public static int FASM_INVALID_PARAMETER = -1;
            public static int FASM_OUT_OF_MEMORY = -2;
            public static int FASM_STACK_OVERFLOW = -3;
            public static int FASM_SOURCE_NOT_FOUND = -4;
            public static int FASM_UNEXPECTED_END_OF_SOURCE = -5;
            public static int FASM_CANNOT_GENERATE_CODE = -6;
            public static int FASM_FORMAT_LIMITATIONS_EXCEEDED = -7;
            public static int FASM_WRITE_FAILED = -8;
            public static int FASM_INVALID_DEFINITION = -9;
        }

        [DllImport("fasm.dll")]
        public static extern IntPtr fasm_Assemble(StringBuilder lpSource, byte[] lpMemory, int cbMemorySize, int nPassesLimit, IntPtr hDisplayPipe);
        [DllImport("fasm.dll")]
        public static extern IntPtr fasm_AssembleFile(StringBuilder lpSourcePath, byte[] lpMemory, int cbMemorySize, int nPassesLimit, IntPtr hDisplayPipe);

        public static string? RunFasmInMemory(StringBuilder generatedAssembly, CompilationOptions options)
        {
            var outputFile = options.OutputPath;
            byte[] memoryBuffer = new byte[options.AssemblerOptions.MemorySize];
            var result = fasm_Assemble(generatedAssembly, memoryBuffer, options.AssemblerOptions.MemorySize, options.AssemblerOptions.PassesLimit, IntPtr.Zero);
            if (result != FasmState.FASM_OK)
            {
                if (result == FasmState.FASM_ERROR) return "FASM_ERROR";
                if (result == FasmState.FASM_INVALID_PARAMETER) return "FASM_INVALID_PARAMETER";
                if (result == FasmState.FASM_OUT_OF_MEMORY) return "FASM_OUT_OF_MEMORY";
                if (result == FasmState.FASM_STACK_OVERFLOW) return "FASM_STACK_OVERFLOW";
                if (result == FasmState.FASM_SOURCE_NOT_FOUND) return "FASM_SOURCE_NOT_FOUND";
                if (result == FasmState.FASM_UNEXPECTED_END_OF_SOURCE) return "FASM_UNEXPECTED_END_OF_SOURCE";
                if (result == FasmState.FASM_CANNOT_GENERATE_CODE) return "FASM_CANNOT_GENERATE_CODE";
                if (result == FasmState.FASM_FORMAT_LIMITATIONS_EXCEEDED) return "FASM_FORMAT_LIMITATIONS_EXCEEDED";
                if (result == FasmState.FASM_WRITE_FAILED) return "FASM_WRITE_FAILED";
                if (result == FasmState.FASM_INVALID_DEFINITION) return "FASM_INVALID_DEFINITION";
                return "FATAL_FASM_ERROR";
            }
            try
            {
                var structResult = new FasmResult
                {
                    Condition = BitConverter.ToInt32(memoryBuffer, 0),
                    OutputLengthOrErrorCode = BitConverter.ToInt32(memoryBuffer, 4),
                    OutputDataOrErrorLine = BitConverter.ToInt32(memoryBuffer, 8),
                };
                var resultBuffer = new byte[structResult.OutputLengthOrErrorCode];
                Marshal.Copy(structResult.OutputDataOrErrorLine, resultBuffer, 0, structResult.OutputLengthOrErrorCode);

                File.WriteAllBytes(outputFile, resultBuffer);
            }
            catch (Exception ex)
            {
                return "executable generation failed";
            }
            return null;
        }
        public static string? RunFasm(CompilationOptions options)
        {
            var assemblyFile = options.AssemblyPath;
            var outputFile = options.OutputPath;
            byte[] memoryBuffer = new byte[options.AssemblerOptions.MemorySize];
            var sourcePathSb = new StringBuilder(options.AssemblyPath);
            var result = fasm_AssembleFile(sourcePathSb, memoryBuffer, options.AssemblerOptions.MemorySize, options.AssemblerOptions.PassesLimit, IntPtr.Zero);
            if (result != FasmState.FASM_OK)
            {
                if (result == FasmState.FASM_ERROR) return "FASM_ERROR";
                if (result == FasmState.FASM_INVALID_PARAMETER) return "FASM_INVALID_PARAMETER";
                if (result == FasmState.FASM_OUT_OF_MEMORY) return "FASM_OUT_OF_MEMORY";
                if (result == FasmState.FASM_STACK_OVERFLOW) return "FASM_STACK_OVERFLOW";
                if (result == FasmState.FASM_SOURCE_NOT_FOUND) return "FASM_SOURCE_NOT_FOUND";
                if (result == FasmState.FASM_UNEXPECTED_END_OF_SOURCE) return "FASM_UNEXPECTED_END_OF_SOURCE";
                if (result == FasmState.FASM_CANNOT_GENERATE_CODE) return "FASM_CANNOT_GENERATE_CODE";
                if (result == FasmState.FASM_FORMAT_LIMITATIONS_EXCEEDED) return "FASM_FORMAT_LIMITATIONS_EXCEEDED";
                if (result == FasmState.FASM_WRITE_FAILED) return "FASM_WRITE_FAILED";
                if (result == FasmState.FASM_INVALID_DEFINITION) return "FASM_INVALID_DEFINITION";
                return "FATAL_FASM_ERROR";
            }
            try
            {
                var structResult = new FasmResult
                {
                    Condition = BitConverter.ToInt32(memoryBuffer, 0),
                    OutputLengthOrErrorCode = BitConverter.ToInt32(memoryBuffer, 4),
                    OutputDataOrErrorLine = BitConverter.ToInt32(memoryBuffer, 8),
                };
                var resultBuffer = new byte[structResult.OutputLengthOrErrorCode];
                Marshal.Copy(structResult.OutputDataOrErrorLine, resultBuffer, 0, structResult.OutputLengthOrErrorCode);

                File.WriteAllBytes(outputFile, resultBuffer);
            } catch (Exception ex)
            {
                return "executable generation failed";
            }
            return null;
        }
    }
}
