using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextMicrocodes.Default)
            .AddSingleton<MicrocodesCombiner>()
            .AddSingleton<CommandHandlers>();

        var command = parentCommand.AddCommand("mcodes-combine", "Combine/inject microcodes to file");

        var inputOpt = command.AddOption(new Option<string>("--input", "-i")
        {
            Description = "Bin file",
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
            Description = "Output file",
            Required = true,
        });

        command.SetAction<CommandHandlers>(services,
            (handler, opts) => handler.CombineMicrocodes(
                opts.GetRequiredValue(inputOpt),
                opts.GetRequiredValue(tableOpt),
                opts.GetRequiredValue(mcodesOpt),
                opts.GetRequiredValue(outputOpt)
            ));
    }
}
