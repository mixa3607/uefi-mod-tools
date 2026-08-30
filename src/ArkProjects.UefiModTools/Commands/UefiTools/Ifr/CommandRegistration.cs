using ArkProjects.UefiModTools.Ifr;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<SetupDataParser>()
            .AddSingleton<IJsonTypeInfoResolver>(IfrJsonSerializerContext.Default)
            .AddSingleton<CommandHandlers>();

        RegisterSetupDataExtract(parentCommand, services);
        RegisterSetupDataPatch(parentCommand, services);
        RegisterSctPatch(parentCommand, services);
    }

    private static void RegisterSetupDataExtract(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("ifr-setupdata-extract", "Extract SetupData to json using IFR dump");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "SetupData binary file",
            Required = true,
        });
        var ifrOpt = command.AddOption(new Option<string>("--ifr", "-s")
        {
            Description = "IFR dump json file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output SetupData json file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.ExtractSetupData(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }

    private static void RegisterSetupDataPatch(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("ifr-setupdata-patch", "Patch SetupData binary file using json patch");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "SetupData binary file",
            Required = true,
        });
        var patchOpt = command.AddOption(new Option<string>("--patch", "-p")
        {
            Description = "Partial edited SetupData json file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output SetupData binary file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.PatchSetupData(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }

    private static void RegisterSctPatch(Command parentCommand, IServiceCollection services)
    {
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
            (handler, opts) => handler.PatchSct(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(ifrOpt),
                opts.GetRequiredValue(patchOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
