using System.CommandLine;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcHpm;

public static class Registration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services.AddSingleton<HpmCommandHandlers>();

        {
            var command = parentCommand.AddCommand("bmc-bios2hpm", "");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Bin file",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output file",
                    Required = true,
                });


            command.SetAction<HpmCommandHandlers>(services,
                (handler, opts) => handler.BuildHpm(
                //opts.GetRequiredValue(inputOpt),
                //opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
