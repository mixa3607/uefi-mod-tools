using System.CommandLine;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BiosDecodeCodes;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<CommandHandlers>()
            ;

        // bios-post-decode
        {
            var command = parentCommand.AddCommand("bios-post-decode", "Decode BIOS post codes");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Space separated hex codes",
                    DefaultValueFactory = _ => "-",
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Output info",
                    DefaultValueFactory = _ => "-",
                });

            command.SetAction<CommandHandlers>(services,
                (handler, opts) => handler.PostDecode(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
