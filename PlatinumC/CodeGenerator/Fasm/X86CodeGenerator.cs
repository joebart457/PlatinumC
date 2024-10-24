﻿using PlatinumC.Compiler;
using PlatinumC.Extensions;
using System.Text;

namespace PlatinumC.CodeGenerator.Fasm
{
    public static class X86CodeGenerator
    {
        public static string? Generate(CompilationResult compilationResult)
        {
            SantizeAssemblyFilePath(compilationResult.CompilationOptions);
            SantizeOutputFilePath(compilationResult.CompilationOptions);
            GenerateAssembly(compilationResult);
            return GenerateExecutable(compilationResult.CompilationOptions);
        }
        private static void GenerateAssembly(CompilationResult data)
        {
            var sb = new StringBuilder();
            if (data.CompilationOptions.OutputTarget == OutputTarget.Exe)
            {
                sb.AppendLine("format PE console");
                sb.AppendLine("entry Main");
            }
            else if (data.CompilationOptions.OutputTarget == OutputTarget.Dll)
            {
                sb.AppendLine("format PE DLL");
                sb.AppendLine("entry DllEntryPoint");
            }
            else throw new Exception($"unable to generate code for output target {data.CompilationOptions.OutputTarget}");

            // Output Resource data
            //sb.AppendLine(data.ResourceData.GenerateAssembly(data.Settings, 0));

            // Output static data
            sb.AppendLine("section '.data' data readable writeable".Indent(0));
            foreach(var stringData in data.StaticStringData)
            {
                sb.AppendLine(stringData.Emit(1));
            }

            foreach (var floatingPointData in data.StaticFloatingPointData)
            {
                sb.AppendLine(floatingPointData.Emit(1));
            }

            // Output User Functions
            sb.AppendLine("section '.text' code readable executable");
            foreach (var proc in data.FunctionData)
            {
                sb.Append(proc.Emit(1));
            }

            // Output imported functions
            sb.AppendLine("section '.idata' import data readable writeable");
            int libCounter = 0;
            foreach(var importLibrary in data.ImportLibraries)
            {
                sb.AppendLine($"dd 0,0,0,RVA !lib_{libCounter}_name, RVA !lib_{libCounter}_table".Indent(1));
                libCounter++;
            }
            sb.AppendLine($"dd 0,0,0,0,0".Indent(1));
            libCounter = 0;
            foreach (var importLibrary in data.ImportLibraries)
            {
                sb.AppendLine($"!lib_{libCounter}_table:".Indent(1));
                foreach(var importedFunction in importLibrary.ImportedFunctions)
                {
                    sb.AppendLine($"{importedFunction.FunctionIdentifier.Lexeme} dd RVA !{importedFunction.FunctionIdentifier.Lexeme}".Indent(1));
                }
                sb.AppendLine($"dd 0".Indent(1));
                libCounter++;
            }
            libCounter = 0;
            foreach (var importLibrary in data.ImportLibraries)
            {
                sb.AppendLine($"!lib_{libCounter}_name db '{importLibrary.LibraryPath.Lexeme}',0".Indent(1));
                libCounter++;
            }

            foreach (var importLibrary in data.ImportLibraries)
            {
                foreach (var importedFunction in importLibrary.ImportedFunctions)
                {
                    sb.AppendLine($"!{importedFunction.FunctionIdentifier.Lexeme} db 0,0,'{importedFunction.Symbol.Lexeme}',0".Indent(1));
                }
            }


            // Output exported user functions
            if (data.CompilationOptions.OutputTarget == OutputTarget.Dll)
            {
                sb.AppendLine("section '.edata' export data readable");

                sb.AppendLine($"dd 0,0,0, RVA !lib_name, 1".Indent(1));
                sb.AppendLine($"dd {data.ExportedFunctions.Count},{data.ExportedFunctions.Count}, RVA !exported_addresses, RVA !exported_names, RVA !exported_ordinals".Indent(1));

                sb.AppendLine($"!exported_addresses:".Indent(1));
                foreach(var exportedFunction in data.ExportedFunctions)
                {
                    sb.AppendLine($"dd RVA {exportedFunction.functionIdentifier}".Indent(2));
                }

                sb.AppendLine($"!exported_names:".Indent(1));
                int exportedNamesCounter = 0;
                foreach (var exportedFunction in data.ExportedFunctions)
                {
                    sb.AppendLine($"dd RVA !exported_{exportedNamesCounter}".Indent(2));
                    exportedNamesCounter++;
                }
                exportedNamesCounter = 0;
                sb.AppendLine($"!exported_ordinals:".Indent(1));
                foreach (var exportedFunction in data.ExportedFunctions)
                {
                    sb.AppendLine($"dw {exportedNamesCounter}".Indent(2));
                    exportedNamesCounter++;
                }

                exportedNamesCounter = 0;
                sb.AppendLine($"!lib_name db '{Path.GetFileName(data.CompilationOptions.OutputPath)}',0".Indent(1));
                foreach (var exportedFunction in data.ExportedFunctions)
                {
                    sb.AppendLine($"!exported_{exportedNamesCounter} db '{exportedFunction.exportedSymbol}',0".Indent(2));
                    exportedNamesCounter++;
                }

            }

            File.WriteAllText(data.CompilationOptions.AssemblyPath, sb.ToString());
        }

        private static string? GenerateExecutable(CompilationOptions options)
        {
            return FasmService.RunFasm(options);
        }


        private static void SantizeAssemblyFilePath(CompilationOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.AssemblyPath))
            {
                options.AssemblyPath = Path.GetTempFileName();
                return;
            }
            options.AssemblyPath = Path.GetFullPath(options.AssemblyPath);
            if (Path.GetExtension(options.AssemblyPath) != ".asm") options.AssemblyPath = $"{options.AssemblyPath}.asm";
        }

        private static void SantizeOutputFilePath(CompilationOptions options)
        {
            if (string.IsNullOrEmpty(options.OutputPath)) options.OutputPath = Path.GetFullPath(Path.GetFileNameWithoutExtension(options.InputPath));
            var outputPath = Path.GetFullPath(options.OutputPath);
            if (options.OutputTarget == OutputTarget.Exe && Path.GetExtension(outputPath) != ".exe") outputPath = $"{outputPath}.exe";
            if (options.OutputTarget == OutputTarget.Dll && Path.GetExtension(outputPath) != ".dll") outputPath = $"{outputPath}.dll";
            options.OutputPath = outputPath;
        }
    }
}
