using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UBootTools.Env;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<UBootEnvParser>()
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextUBootEnv.Default)
            .AddSingleton<CommandHandlers>()
            ;

        // scan4env
        {
            var command = parentCommand.AddCommand("env-scan", "Try find UBoot env section in dump file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Firmware dump",
                    Required = true,
                });
            var blkSizeOpt = command.AddOption(
                new Option<int>("--blk-size", "-s")
                {
                    Description = "Block size",
                    CustomParser = ArgumentParsers.NumberParser<int>,
                    DefaultValueFactory = _ => 0x10000,
                });
            var windowBlksOpt = command.AddOption(
                new Option<int>("--windows-blks", "-w")
                {
                    Description = "Sliding window in blocks",
                    CustomParser = ArgumentParsers.NumberParser<int>,
                    DefaultValueFactory = _ => 1,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output json file",
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.Scan(
                    opts.GetRequiredValue(inputOpt),
                    [opts.GetRequiredValue(blkSizeOpt)],
                    [opts.GetRequiredValue(windowBlksOpt)],
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        // unpack
        {
            var command = parentCommand.AddCommand("env-read", "Parse UBoot env bin section to json");

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
            var command = parentCommand.AddCommand("env-write", "Write UBoot env bin section from json file");

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
