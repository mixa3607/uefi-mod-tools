using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSct;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextIfrSct.Default)
            .AddSingleton<CommandHandlers>();

        var command = parentCommand.AddCommand("ifr-sct-patch", "Patch Platform_setup.sct using IFR dump and json patch");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "Platform_setup.sct file",
            Required = true,
        });
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump json file",
            Required = true,
        });
        var patchOpt = command.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "IFR patch json file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output Platform_setup.sct file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.Patch(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
