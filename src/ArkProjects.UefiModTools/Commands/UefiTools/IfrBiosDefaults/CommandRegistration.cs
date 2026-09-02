using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using ArkProjects.UefiModTools.Utils;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaults;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextIfrBiosDefaults.Default)
            .AddSingleton<NvarMapExtractor>()
            .AddSingleton<BiosDefaultsStoreMapper>()
            .AddSingleton<CommandHandlers>();

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

        extractCommand.SetAction<CommandHandlers>(services,
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

        mapStoreCommand.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.MapStore(
                opts.GetRequiredValue(mapInputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(mapOutputOpt),
                opts.GetValue(ignoreVersionsOpt)
            ));
    }
}
