using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public static class FitCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(FitCommandJsonSerializerContext.Default)
            .AddSingleton<FitParser>()
            .AddSingleton<FitMapper>()
            .AddSingleton<FitPatchApplier>()
            .AddSingleton<FitCommandHandlers>();

        var fitCommand = parentCommand.AddCommand("fit", "Firmware Interface Table tools");
        RegisterMap(fitCommand, services);
        RegisterApplyPatch(fitCommand, services);
    }

    private static void RegisterMap(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("map", "Map FIT entries");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "FIT binary file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output FIT map file",
            Required = true,
        });
        var outputFormatOpt = command.AddFileFormatOption(outputOpt);
        command.SetAction<FitCommandHandlers>(services,
            (handler, opts) => handler.Map(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetRequiredValue(outputFormatOpt)
            ));
    }

    private static void RegisterApplyPatch(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("apply-patch", "Apply a FIT patch");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "FIT binary file",
            Required = true,
        });
        var mapOpt = command.AddOption(new Option<string>("--map", "-m")
        {
            Description = "FIT map JSON file",
            Required = true,
        });
        var patchOpt = command.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "FIT patch JSON file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output FIT binary file",
            Required = true,
        });
        var ignoreVersionsOpt = command.AddOption(new Option<bool>("--ignore-versions")
        {
            Description = "Allow unsupported FIT map and patch versions",
        });
        var ignoreChecksumsOpt = command.AddOption(new Option<bool>("--ignore-checksums")
        {
            Description = "Allow a FIT input that does not match the map source hash",
        });

        command.SetAction<FitCommandHandlers>(services,
            (handler, opts) => handler.ApplyPatch(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(mapOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetValue(ignoreVersionsOpt),
                opts.GetValue(ignoreChecksumsOpt)
            ));
    }
}
