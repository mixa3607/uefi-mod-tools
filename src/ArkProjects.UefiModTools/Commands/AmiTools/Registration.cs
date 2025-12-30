using System.CommandLine;
using ArkProjects.UefiModTools.Commands.AmiTools.Baks;
using ArkProjects.UefiModTools.Commands.AmiTools.Fmh;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools;

public class Registration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("ami", "AMI bin dumps related tools");
        parentCommand.Add(command);

        RegisterBmc(command, services);
    }

    public static void RegisterBmc(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("bmc", "AMI BMC bin dumps related tools");
        parentCommand.Add(command);

        RegisterBmcBackup(command, services);
        RegisterBmcFmh(command, services);
    }

    private static void RegisterBmcFmh(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<FmhCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("fmh-extract", "Extract FMH from BMC dump");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Bin file",
                Required = true,
            };
            command.Add(inputOpt);

            var blkSizeOpt = new Option<int>("--blk-size", "-s")
            {
                Description = "Block size",
                CustomParser = ArgumentParsers.NumberParser<int>,
                DefaultValueFactory = _ => 0x10000,
            };
            command.Add(blkSizeOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<FmhCommandHandlers>();
                handler.ExtractFmh(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(blkSizeOpt)
                );
            });
        }
    }

    private static void RegisterBmcBackup(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<AmiConfigBackupParser>()
                    .AddSingleton<BakCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("unpack-bak",
                "Unpack config.bak file exported from BMC web ui with sign verification");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Bin file",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output files directory",
                Required = true,
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<BakCommandHandlers>();
                handler.UnpackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }

        // pack
        {
            var command = new Command("pack-bak", "Pack and sign files to config.bak that can be imported to BMC");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "files directory file",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output file",
                Required = true,
            };
            command.Add(outputOpt);

            var buggedShaOpt = new Option<bool>("--bugged-sha")
            {
                Description = "Set if your BMC have bugged sign implementation.\n" +
                              "== Axiomtek IMB760 - true\n" +
                              "== Lenovo RD450x - false",
                DefaultValueFactory = _ => false,
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<BakCommandHandlers>();
                handler.PackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(buggedShaOpt)
                );
            });
        }
    }
}
