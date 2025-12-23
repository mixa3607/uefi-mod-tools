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

        // parse
        rootCommand.SetAction(_ => Test(services.BuildServiceProvider()));//
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

    private static void Test(IServiceProvider services)
    {
        var jOpts = services.GetRequiredService<JsonSerializerOptions>();
        var jStr = File.ReadAllText("test-files/ServerMgmtSetup_data.json");
        var j = JsonSerializer.Deserialize<Commands.UefiEditorJs.Data>(jStr, jOpts);

        var biosTree = new List<BiosSection>();
        foreach (var menu in j.Menu)
        {
            var section = new BiosSection()
            {
                Name = menu.Name,
                Description = "",
                Type = "menu"
            };
            biosTree.Add(section);
            ProcessForm(menu.FormId, j, section, 0);
        }

        var js = JsonSerializer.Serialize(biosTree, jOpts);
    }

    private static void ProcessForm(string formId, Data data, BiosSection parent, int depth)
    {
        if (depth > 10)
        {
            return;
        }
        var form = data.Forms.First(x => x.FormId == formId);
        foreach (var child in form.Children)
        {
            if (child is { FormId: not null, Type: "Ref" } && form.FormId == child.FormId)
            {
                return;
            }
            var section = new BiosSection()
            {
                Type = child.Type,
                Name = child.Name,
                Description = child.Description,
            };
            parent.Childs.Add(section);
            if (child is { FormId: not null, Type: "Ref" })
            {
                ProcessForm(child.FormId, data, section, depth+1);
            }
        }
    }
}

public class BiosSection
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public List<BiosSection> Childs { get; set; } = [];
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
