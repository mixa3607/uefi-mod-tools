using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("uboot", "UBoot related tools");
        RegisterUBootEnvCommands(command, services);
    }

    private static void RegisterUBootEnvCommands(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<UBootEnvParser>()
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextUBoot.Default)
            .AddSingleton<CommandHandlers>()
            ;

        // unpack
        {
            var command = parentCommand.AddCommand("read-env", "Parse UBoot env bin section to json");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "UBoot env file",
                    Required = true,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output json file",
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.Read(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        // pack
        {
            var command = parentCommand.AddCommand("write-env", "Write UBoot env bin section from json file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Json file",
                    Required = true,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "UBoot env file",
                    Required = true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.Write(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
