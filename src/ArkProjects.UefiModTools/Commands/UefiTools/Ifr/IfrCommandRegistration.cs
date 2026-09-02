using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public static class IfrCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(IfrJsonSerializerContext.Default)
            .AddSingleton<IfrDocumentRenderer>()
            .AddSingleton<IfrCommandHandlers>();

        var ifrCommand = parentCommand.AddCommand("ifr", "IFR tools");
        var command = ifrCommand.AddCommand("render", "Render an IFR dump to an IFR document");
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump json file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output file",
            DefaultValueFactory = _ => "-",
        });
        command.SetAction<IfrCommandHandlers>(services,
            (handler, opts) => handler.Render(
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
