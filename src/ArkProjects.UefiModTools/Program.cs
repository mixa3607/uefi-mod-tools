using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.CommandLine;
using ArkProjects.UefiModTools.Misc;

namespace ArkProjects.UefiModTools;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        // init
        var services = new ServiceCollection();

        // reg commands
        var rootCommand = new RootCommand("UEFI related mod tools");
        var logLevelOpt = new Option<LogLevel>("--log-level", "-l")
        {
            Description = "Logging level",
            DefaultValueFactory = _ => LogLevel.Information,
        };
        rootCommand.Add(logLevelOpt);

        Commands.UefiEditorJsTools.CommandRegistration.Register(rootCommand, services);
        Commands.BinTools.CommandRegistration.Register(rootCommand, services);
        Commands.SmbiosTools.CommandRegistration.Register(rootCommand, services);
        Commands.AmiTools.CommandRegistration.Register(rootCommand, services);
        Commands.UBootTools.CommandRegistration.Register(rootCommand, services);

        // parse
        var parseResult = rootCommand.Parse(args);

        // reg services
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .CreateLogger();
        services.AddLogging(b => b
            .AddSerilog()
            .SetMinimumLevel(parseResult.GetValue(logLevelOpt)));
        services.AddSingleton<JsonSerializationService>();

        // exec
        return await parseResult.InvokeAsync(new InvocationConfiguration()
        {
            Error = Console.Error,
            Output = Console.Error,
        });
    }
}
