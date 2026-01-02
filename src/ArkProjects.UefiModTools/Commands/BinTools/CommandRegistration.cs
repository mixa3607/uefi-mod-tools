using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.BinTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("bin", "Bin dumps related tools");
        parentCommand.Add(command);

        RegisterRenderCommands(command, services);
    }

    private static void RegisterRenderCommands(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextBinTools.Default)
            .AddSingleton<CommandHandlers>()
            ;

        // split
        {
            var command = parentCommand.AddCommand("split", "Split dump by partition table");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file",
                    DefaultValueFactory = _ => "dump.bin"
                });

            var tableOpt = command.AddOption(
                new Option<string>("--table", "-t")
                {
                    Description = "Partitions table json",
                    DefaultValueFactory = _ => "partitions.json"
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output files directory",
                    Required = true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.SplitBin(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(tableOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        // combine
        {
            var command = parentCommand.AddCommand("combine", "Combine/inject partitions to file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file for injection",
                    DefaultValueFactory = _ => "dump.bin"
                });

            var tableOpt = command.AddOption(
                new Option<string>("--table", "-t")
                {
                    Description = "Partitions table json",
                    DefaultValueFactory = _ => "partitions.json"
                });

            var partitionsOpt = command.AddOption(
                new Option<string>("--partitions", "-p")
                {
                    Description = "Partitions directory",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output file",
                    DefaultValueFactory = _ => "./extract",
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.CombineBin(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(tableOpt),
                    opts.GetRequiredValue(partitionsOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
