using ArkProjects.UefiModTools.Commands.Smbios;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiEditorJs;
using Serilog;
using Serilog.Events;

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
        SmbiosCommandRegistration.Register(rootCommand, services);
        UefiEditorJsCommandRegistration.Register(rootCommand, services);

        // parse
        //rootCommand.SetAction(_ => Test(services.BuildServiceProvider()));//
        var parseResult = rootCommand.Parse(args);

        // reg services
        services.AddSingleton(new JsonSerializerOptions
        {
            TypeInfoResolver = JsonSourceGenerationContext.Default,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(),
                // new ByteArrayConverter()
            },
        });

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console(standardErrorFromLevel: LogEventLevel.Verbose)
            .CreateLogger();
        services.AddLogging(b => b
            .AddSerilog()
            .SetMinimumLevel(parseResult.GetValue(logLevelOpt)));

        // exec
        return await parseResult.InvokeAsync(new InvocationConfiguration()
        {
            Error = Console.Error,
            Output = Console.Out,
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
