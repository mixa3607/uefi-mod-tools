using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrRender.HtmlViewer;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextIfrRender.Default)
            .AddSingleton<IfrTreeRenderer>()
            .AddSingleton<IfrHtmlViewerRenderer>()
            .AddSingleton<CommandHandlers>();

        var command = parentCommand.AddCommand("ifr-render", "Render IFR data from Platform_setup.sct and SetupData");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "Platform_setup.sct file",
            Required = true,
        });
        var setupDataOpt = command.AddOption(new Option<string>("--setup-data")
        {
            Description = "SetupData binary file",
            Required = true,
        });
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump json file",
            Required = true,
        });
        var formatOpt = command.AddOption(new Option<string>("--format", "-f")
        {
            Description = "Output format: json, html, or ascii-tree",
            DefaultValueFactory = _ => "json",
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output file",
            DefaultValueFactory = _ => "-",
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.Render(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(setupDataOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(formatOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
