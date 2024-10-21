

using CliParser;
using Logger;
using PlatinumC.Services;

var startupService = new StartupService();

args = ["C:\\Users\\Jimmy\\Desktop\\Repositories\\PlatinumC\\PlatinumC\\Assets\\syntax_example.txt", "C:\\Users\\Jimmy\\Desktop\\Repositories\\PlatinumC\\PlatinumC\\bin\\Debug\\net8.0\\test.exe", "-n", "0", "-a", "C:\\Users\\Jimmy\\Desktop\\Repositories\\PlatinumC\\PlatinumC\\bin\\Debug\\net8.0\\asmtest.asm"];


args.ResolveWithTryCatch(startupService, ex =>
{
    CliLogger.LogError(ex.InnerException?.Message ?? $"fatal error: {ex.Message}");
});