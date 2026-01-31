using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("uefi", "UEFI related tools");
        parentCommand.Add(command);

        RegisterMCodesCommands(command, services);
        RegisterFitCommands(command, services);
    }

    private static void RegisterMCodesCommands(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextUefiTools.Default)
            .AddSingleton<CommandHandlers>()
            ;

        {
            var command = parentCommand.AddCommand("mcodes-combine", "Combine/inject microcodes to file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file",
                    Required = true,
                });

            var tableOpt = command.AddOption(
                new Option<string>("--table", "-t")
                {
                    Description = "Microcodes table json",
                    DefaultValueFactory = _ => "microcodes.json"
                });

            var mcodesOpt = command.AddOption(
                new Option<string>("--mcodes", "-m")
                {
                    Description = "Microcodes directory",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
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

    private static void RegisterFitCommands(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextUefiTools.Default)
            .AddSingleton<CommandHandlers>()
            .AddSingleton<FitParser>()
            ;


        // read
        {
            var command = parentCommand.AddCommand("fit-read", "Parse FIT bin section to json");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "FIT file",
                    Required = true,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output json file",
                    DefaultValueFactory = _ => "-"
                });
            var verifyOpt = command.AddOption(
                new Option<bool>("--verify")
                {
                    Description = "Verify that final json will be same after convert to table",
                    DefaultValueFactory = _ => true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.ReadFit(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(verifyOpt)
                ));
        }

        // write
        {
            var command = parentCommand.AddCommand("fit-write", "Write FIT bin section from json file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Json file",
                    Required = true,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "FIT file",
                    Required = true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.WriteFit(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        {
            var command = parentCommand.AddCommand("fit-inject-mcodes", "Inject microcodes to fit file");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "FIT file",
                    Required = true,
                });

            var tableOpt = command.AddOption(
                new Option<string>("--table", "-t")
                {
                    Description = "Microcodes table json",
                    DefaultValueFactory = _ => "microcodes.json"
                });

            var mcodesOpt = command.AddOption(
                new Option<string>("--mcodes", "-m")
                {
                    Description = "Partitions directory",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output FIT file",
                    Required = true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.InjectMicrocodes2Fit(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(tableOpt),
                    opts.GetRequiredValue(mcodesOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
