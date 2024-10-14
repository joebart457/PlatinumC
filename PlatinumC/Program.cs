

using CliParser;
using PlatinumC.Services;

args = ["C:\\Users\\Jimmy\\Desktop\\Repositories\\PlatinumC\\PlatinumC\\Assets\\dtest.txt", "test.exe", "optimized_test.asm"];

var startupService = new StartupService();
args.ResolveWithTryCatch(startupService, ex => Console.WriteLine(ex.InnerException?.StackTrace));