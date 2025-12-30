using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.BinTools;

public class BinCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("bin", "Bin dumps related tools");
        parentCommand.Add(command);

        RegisterRenderCommands(command, services);
    }

    private static void RegisterRenderCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<IJsonTypeInfoResolver>(Bin.BinJsonSerializerContext.Default)
                    .AddSingleton<BinCommandHandlers>()
                ;
        }

        // split
        {
            var command = new Command("split", "Split dump by partition table");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Bin file",
                DefaultValueFactory = _ => "dump.bin"
            };
            command.Add(inputOpt);

            var tableOpt = new Option<string>("--table", "-t")
            {
                Description = "Partitions table json",
                DefaultValueFactory = _ => "partitions.json"
            };
            command.Add(tableOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output files directory",
                Required = true,
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<BinCommandHandlers>();
                handler.SplitBin(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(tableOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }

        // split
        {
            var command = new Command("combine", "Combine/inject partitions to file");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Bin file for injection",
                DefaultValueFactory = _ => "dump.bin"
            };
            command.Add(inputOpt);

            var tableOpt = new Option<string>("--table", "-t")
            {
                Description = "Partitions table json",
                DefaultValueFactory = _ => "partitions.json"
            };
            command.Add(tableOpt);

            var partitionsOpt = new Option<string>("--partitions", "-p")
            {
                Description = "Partitions directory",
                Required = true,
            };
            command.Add(partitionsOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Output file",
                DefaultValueFactory = _ => "./extract",
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<BinCommandHandlers>();
                handler.CombineBin(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(tableOpt),
                    opts.GetRequiredValue(partitionsOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }
}
