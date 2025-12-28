using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools;

public class AmiToolsCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("ami-tools", "Bin dumps related tools");
        parentCommand.Add(command);

        RegisterRenderCommands(command, services);
    }

    private static void RegisterRenderCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<AmiToolsCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("unpack-bak", "");
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
                var handler = di.GetRequiredService<AmiToolsCommandHandlers>();
                handler.UnpackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }

        // pack
        {
            var command = new Command("pack-bak", "");
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

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<AmiToolsCommandHandlers>();
                handler.PackBak(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }
}
