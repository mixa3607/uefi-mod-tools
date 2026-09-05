using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct;

public static class SctCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(SctJsonSerializerContext.Default)
            .AddSingleton<SctPatchApplier>()
            .AddSingleton<SctCommandHandlers>();

        var sctCommand = parentCommand.AddCommand("sct", "Platform_setup.sct tools");
        var command = sctCommand.AddCommand("apply-patch", "Apply an SCT patch using an IFR dump");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "Platform_setup.sct file",
            Required = true,
        });
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump file",
            Required = true,
        });
        var patchOpt = command.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "IFR patch file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output Platform_setup.sct file",
            Required = true,
        });
        var ignoreVersionsOpt = command.AddIgnoreVersionsOption(
            description: "Allow unsupported IFR extractor and SCT patch versions");

        command.SetAction<SctCommandHandlers>(services,
            (handler, opts) => handler.Patch(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetValue(ignoreVersionsOpt)
            ));
    }
}
