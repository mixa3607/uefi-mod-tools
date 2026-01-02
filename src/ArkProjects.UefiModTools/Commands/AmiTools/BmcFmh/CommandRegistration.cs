using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextAmiToolsBmcFmh.Default)
            .AddSingleton<CommandHandlers>()
            ;

        // bmc-fmh-scan
        {
            var command = parentCommand.AddCommand("bmc-fmh-scan", "Scan FMH structures in AMI BMC dump");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file",
                    Required = true,
                });

            var blkSizeOpt = command.AddOption(
                new Option<int>("--blk-size", "-s")
                {
                    Description = "Block size",
                    CustomParser = ArgumentParsers.NumberParser<int>,
                    DefaultValueFactory = _ => 0x10000,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Scan result in json format",
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.ScanFmh(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(blkSizeOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
