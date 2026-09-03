using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using ArkProjects.UefiModTools.Utils;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Nvar;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults;

public static class BiosDefaultsCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(BiosDefaultsJsonSerializerContext.Default)
            .AddSingleton<NvarMapExtractor>()
            .AddSingleton<BiosDefaultsIfrMapper>()
            .AddSingleton<BiosDefaultsPatchApplier>()
            .AddSingleton<BiosDefaultsCommandHandlers>();

        var nvarCommand = parentCommand.AddCommand("nvar", "NVAR defaults tools");

        var extractCommand = nvarCommand.AddCommand(
            "map",
            "Create an NVAR record map from a BIOS defaults stream");
        var inputOpt = extractCommand.AddOption(new Option<string>("--input", "-i")
        {
            Description = "BIOS defaults binary file",
            Required = true,
        });
        var outputOpt = extractCommand.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output BIOS defaults map JSON file",
            Required = true,
        });

        extractCommand.SetAction<BiosDefaultsCommandHandlers>(services,
            (handler, opts) => handler.Extract(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(outputOpt)
            ));

        var mapStoreCommand = nvarCommand.AddCommand(
            "map-ifr-stores",
            "Map NVAR records to IFR VarStores and questions");
        var mapInputOpt = mapStoreCommand.AddOption(new Option<string>("--input", "-i")
        {
            Description = "BIOS defaults NVAR map JSON file",
            Required = true,
        });
        var ifrOpt = mapStoreCommand.AddOption(new Option<string>("--ifr")
        {
            Description = "IFR dump JSON file",
            Required = true,
        });
        var mapOutputOpt = mapStoreCommand.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output BIOS defaults store map JSON file",
            Required = true,
        });
        var ignoreVersionsOpt = mapStoreCommand.AddOption(new Option<bool>("--ignore-versions")
        {
            Description = "Allow unsupported BIOS defaults map and IFR extractor versions",
        });

        mapStoreCommand.SetAction<BiosDefaultsCommandHandlers>(services,
            (handler, opts) => handler.MapStore(
                opts.GetRequiredValue(mapInputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(mapOutputOpt),
                opts.GetValue(ignoreVersionsOpt)
            ));

        var applyPatchCommand = nvarCommand.AddCommand("apply-patch", "Apply an NVAR question patch to a BIOS defaults stream");
        var patchInputOpt = applyPatchCommand.AddOption(new Option<string>("--input", "-i")
        {
            Description = "BIOS defaults binary file",
            Required = true,
        });
        var storeMapOpt = applyPatchCommand.AddOption(new Option<string>("--map", "-m")
        {
            Description = "BIOS defaults store map JSON file",
            Required = true,
        });
        var patchOpt = applyPatchCommand.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "BIOS defaults store patch JSON file",
            Required = true,
        });
        var patchOutputOpt = applyPatchCommand.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output BIOS defaults binary file",
            Required = true,
        });
        var ignorePatchVersionsOpt = applyPatchCommand.AddOption(new Option<bool>("--ignore-versions")
        {
            Description = "Allow unsupported BIOS defaults store map and patch versions",
        });
        var ignoreChecksumsOpt = applyPatchCommand.AddOption(new Option<bool>("--ignore-checksums")
        {
            Description = "Allow a BIOS defaults input that does not match the map source hash",
        });

        applyPatchCommand.SetAction<BiosDefaultsCommandHandlers>(services,
            (handler, opts) => handler.ApplyPatch(
                opts.GetRequiredValue(patchInputOpt),
                opts.GetRequiredValue(storeMapOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(patchOutputOpt),
                opts.GetValue(ignorePatchVersionsOpt),
                opts.GetValue(ignoreChecksumsOpt)
            ));
    }
}
