using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.ManifestVer;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.CommandLine;

namespace ArkProjects.UefiModTools;

public class Program
{
    public static async Task<int> Main(string[] args)
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

        Commands.BinTools.CommandRegistration.Register(rootCommand, services);
        Commands.SmbiosTools.CommandRegistration.Register(rootCommand, services);
        Commands.AmiTools.CommandRegistration.Register(rootCommand, services);
        Commands.UBootTools.CommandRegistration.Register(rootCommand, services);
        Commands.UefiTools.CommandRegistration.Register(rootCommand, services);

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
            .AddSingleton<ISerializationService, SerializationService>()
            .AddSingleton<ICommandFileManager, CommandFileManager>()
            .AddSingleton<IManifestVersionVerifier, ManifestVersionVerifier>()
            ;

        // exec
        return await parseResult.InvokeAsync(new InvocationConfiguration()
        {
            Error = Console.Error,
            Output = Console.Error,
        });
    }
}
