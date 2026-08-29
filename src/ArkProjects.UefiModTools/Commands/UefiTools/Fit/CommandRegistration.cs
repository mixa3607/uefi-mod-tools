using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextFit.Default)
            .AddSingleton<FitParser>()
            .AddSingleton<FitMicrocodesInjector>()
            .AddSingleton<CommandHandlers>();

        RegisterRead(parentCommand, services);
        RegisterWrite(parentCommand, services);
        RegisterInjectMicrocodes(parentCommand, services);
    }

    private static void RegisterRead(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("fit-read", "Parse FIT bin section to json");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "FIT file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output json file",
            DefaultValueFactory = _ => "-",
        });
        var verifyOpt = command.AddOption(new Option<bool>("--verify")
        {
            Description = "Verify that final json will be same after convert to table",
            DefaultValueFactory = _ => true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.Read(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(outputOpt),
                opts.GetRequiredValue(verifyOpt)
            ));
    }

    private static void RegisterWrite(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("fit-write", "Write FIT bin section from json file");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "Json file",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "FIT file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.Write(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }

    private static void RegisterInjectMicrocodes(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("fit-inject-mcodes", "Inject microcodes to fit file");
        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "FIT file",
            Required = true,
        });
        var tableOpt = command.AddOption(new Option<string>("--table", "-t")
        {
            Description = "Microcodes table json",
            DefaultValueFactory = _ => "microcodes.json",
        });
        var mcodesOpt = command.AddOption(new Option<string>("--mcodes", "-m")
        {
            Description = "Microcodes directory",
            Required = true,
        });
        var outputOpt = command.AddOption(new Option<string>("--output", "-o")
        {
            Description = "Output FIT file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.InjectMicrocodes(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(tableOpt),
                opts.GetRequiredValue(mcodesOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
