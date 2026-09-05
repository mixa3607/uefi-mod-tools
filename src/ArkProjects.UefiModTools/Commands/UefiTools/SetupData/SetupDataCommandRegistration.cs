using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Patching;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData;

public static class SetupDataCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<SetupDataIfrMapper>()
            .AddSingleton<SetupDataPatchApplier>()
            .AddSingleton<IJsonTypeInfoResolver>(SetupDataJsonSerializerContext.Default)
            .AddSingleton<SetupDataCommandHandlers>();

        var setupDataCommand = parentCommand.AddCommand("setup-data", "AMI SetupData tools");
        RegisterSetupDataMapIfr(setupDataCommand, services);
        RegisterSetupDataApplyPatch(setupDataCommand, services);
    }

    private static void RegisterSetupDataMapIfr(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("map-ifr", "Map AMI SetupData questions using an IFR dump");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "SetupData binary file",
            Required = true,
        });
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output SetupData map file",
            Required = true,
        });
        var outputFormatOpt = command.AddFileFormatOption(outputOpt);

        command.SetAction<SetupDataCommandHandlers>(services,
            (handler, opts) => handler.MapIfr(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetRequiredValue(outputFormatOpt)
            ));
    }

    private static void RegisterSetupDataApplyPatch(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("apply-patch", "Apply an AMI SetupData patch");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "SetupData binary file",
            Required = true,
        });
        var mapOpt = command.AddOption(new Option<string>("--map", "-m")
        {
            Description = "SetupData map file",
            Required = true,
        });
        var patchOpt = command.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "SetupData patch file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output SetupData binary file",
            Required = true,
        });
        var ignoreVersionsOpt = command.AddOption(new Option<bool>("--ignore-versions")
        {
            Description = "Allow unsupported SetupData map and patch versions",
        });
        var ignoreChecksumsOpt = command.AddOption(new Option<bool>("--ignore-checksums")
        {
            Description = "Allow a SetupData input that does not match the map source hash",
        });

        command.SetAction<SetupDataCommandHandlers>(services,
            (handler, opts) => handler.PatchSetupData(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(mapOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetValue(ignoreVersionsOpt),
                opts.GetValue(ignoreChecksumsOpt)
            ));
    }

}
