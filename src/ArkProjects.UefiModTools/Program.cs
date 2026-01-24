using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.CommandLine;

namespace ArkProjects.UefiModTools;

internal class Program
{
    static async Task<int> Main(string[] args)
    {
        // init
        var services = new ServiceCollection();

        // reg commands
        var rootCommand = new RootCommand("UEFI related mod tools");
        var logLevelOpt = new Option<LogEventLevel>("--log-level", "-l")
        {
            Description = "Logging level",
            DefaultValueFactory = _ => LogEventLevel.Information,
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
        services.AddLogging(b =>
        {
            var logger = new LoggerConfiguration()
                .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
                .MinimumLevel.Is(parseResult.GetValue(logLevelOpt))
                .CreateLogger();
            b.AddSerilog(logger);
        });
        services
            .AddSingleton<IJsonSerializationService, JsonSerializationService>()
            .AddSingleton<ICommandFileManager, CommandFileManager>()
            ;

        // exec
        return await parseResult.InvokeAsync(new InvocationConfiguration()
        {
            Error = Console.Error,
            Output = Console.Error,
        });
    }
}
