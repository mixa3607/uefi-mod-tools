using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System.CommandLine;
using System.CommandLine.Invocation;
using ArkProjects.UefiModTools.Commands.BinTools;
using ArkProjects.UefiModTools.Commands.SmbiosTools;
using ArkProjects.UefiModTools.Commands.UefiEditorJsTools;
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

        UefiEditorJsCommandRegistration.Register(rootCommand, services);
        BinCommandRegistration.Register(rootCommand, services);
        Registration.Register(rootCommand, services);
        Commands.AmiTools.Registration.Register(rootCommand, services);
        Commands.UBootTools.Registration.Register(rootCommand, services);

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

public class NonTerminatingCommandLineAction : SynchronousCommandLineAction
{
    public override bool Terminating => false;
    private readonly Action<ParseResult> _action;

    public NonTerminatingCommandLineAction(Action<ParseResult> action)
    {
        _action = action;
    }

    public override int Invoke(ParseResult parseResult)
    {
        _action.Invoke(parseResult);
        return 1;
    }
}
