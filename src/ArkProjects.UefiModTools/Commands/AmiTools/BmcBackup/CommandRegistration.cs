using System.CommandLine;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<AmiConfigBackupParser>()
            .AddSingleton<CommandHandlers>()
            ;

        // bmc-backup-extract
        {
            var command = parentCommand.AddCommand("bmc-backup-extract",
                "Extract config.bak file exported from BMC web ui with sign verification");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output files directory",
                    Required = true,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.UnpackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        // bmc-backup-pack
        {
            var command = new Command("bmc-backup-pack",
                "Pack and sign files to config.bak that can be imported to BMC");
            parentCommand.Add(command);

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "files directory file",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output file",
                    Required = true,
                });

            var buggedShaOpt = command.AddOption(
                new Option<bool>("--bugged-sha")
                {
                    Description = "Set if your BMC have bugged sign implementation.\n" +
                                  "== Axiomtek IMB760 - true\n" +
                                  "== Lenovo RD450x - false",
                    DefaultValueFactory = _ => false,
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.PackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(buggedShaOpt)
                ));
        }
    }
}
