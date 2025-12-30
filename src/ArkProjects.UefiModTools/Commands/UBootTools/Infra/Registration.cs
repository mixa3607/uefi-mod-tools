using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class Registration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("uboot", "UBoot related tools");
        parentCommand.Add(command);

        RegisterUBootEnvCommands(command, services);
    }

    private static void RegisterUBootEnvCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<UBootEnvParser>()
                    .AddSingleton<IJsonTypeInfoResolver>(StaticJsonSerializerContext.Default)
                    .AddSingleton<UBootEnvCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("read-env");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "UBoot env file",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output json file",
                Required = true,
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<UBootEnvCommandHandlers>();
                handler.Read(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }

        // pack
        {
            var command = new Command("write-env");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "UBoot env file",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output json file",
                Required = true,
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<UBootEnvCommandHandlers>();
                handler.Write(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }
}
