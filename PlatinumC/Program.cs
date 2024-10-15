

using CliParser;
using Logger;
using PlatinumC.Services;

var startupService = new StartupService();

args.ResolveWithTryCatch(startupService, ex =>
{
    CliLogger.LogError(ex.InnerException?.Message ?? $"fatal error: {ex.Message}");
});