using PlatinumC.Compiler;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlatinumC.CodeGenerator.Fasm
{
    internal static class FasmService
    {
        public static string FasmPath => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Assemblers\\Fasm\\fasm.exe";
        public static string FasmDirectory => $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\\Assemblers\\Fasm\\";

        public static string? RunFasm(CompilationOptions options)
        {
            FasmDllService.RunFasm(options);
            var assemblyFile = options.AssemblyPath;
            var outputFile = options.OutputPath;

            var startInfo = new ProcessStartInfo
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                FileName = $"\"{FasmPath}\" {string.Join(' ', $"\"{assemblyFile}\"", $"\"{outputFile}\"")}",
                CreateNoWindow = true,
                WorkingDirectory = FasmDirectory,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc?.WaitForExit();
            if (proc == null) return "unable to start fasm.exe";
            if (proc.ExitCode != 0) return $"fasm error:\r\n{startInfo.Arguments}\r\n{string.Join("\r\n..", ReadAllLines(proc.StandardError))}";
            return null;
        }



        private static IEnumerable<string> ReadAllLines(StreamReader streamReader)
        {
            var lines = new List<string>();
            var line = "";
            while ((line = streamReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            return lines;
        }
    }
}
