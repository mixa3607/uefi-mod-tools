using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using ArkProjects.UefiModTools.Utils;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextIfrBiosDefaults.Default)
            .AddSingleton<NvarMapExtractor>()
            .AddSingleton<CommandHandlers>();

        var command = parentCommand.AddCommand(
            "ifr-extdefaults-extract",
            "Extract IFR external BIOS defaults");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "BIOS defaults binary file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output BIOS defaults map JSON file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.Extract(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
