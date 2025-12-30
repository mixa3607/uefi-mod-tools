using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Smbios;
using ArkProjects.UefiModTools.Smbios.Structures;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.SmbiosTools;

public class Registration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("smbios", "SMBIOS tables related tools");
        parentCommand.Add(command);

        RegisterTableCommands(command, services);
        RegisterStructureCommands(command, services);
    }

    private static void RegisterTableCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<IJsonTypeInfoResolver>(SmbiosJsonSerializerContext.Default)
                    .AddSingleton<SmbiosReader>()
                    .AddSingleton<SmbiosWriter>()
                    .AddSingleton<SmbiosTableCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("table2json", "Parse SMBIOS table to RAW structures");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description =
                    "Dump of 'File:SMbiosStaticData[DAF4BF89-CE71-4917-B522-C89D32FBC59F]' => 'Section:[*]' body",
                Required = true,
            };
            command.Add(inputOpt);
            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Json representation output file",
                DefaultValueFactory = _ => "smbios-table.json",
            };
            command.Add(outputOpt);

            var verifyOpt = new Option<bool>("--verify")
            {
                Description = "Verify that final json will be same after convert to table",
                DefaultValueFactory = _ => true,
            };
            command.Add(verifyOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<SmbiosTableCommandHandlers>();
                handler.Table2Json(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(verifyOpt)
                );
            });
        }

        // pack
        {
            var command = new Command("json2table", "Convert json dump to SMBIOS table bin");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Json representation of SMBIOS table input file",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description =
                    "Binary for 'File:SMbiosStaticData[DAF4BF89-CE71-4917-B522-C89D32FBC59F]' => 'Section:[*]' body",
                DefaultValueFactory = _ => "-"
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<SmbiosTableCommandHandlers>();
                handler.Json2Table(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }

    private static void RegisterStructureCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<IJsonTypeInfoResolver>(SmbiosJsonSerializerContext.Default)
                    .AddSingleton<BiosInformationConverter>()
                    .AddSingleton<ISmbiosStructureReader>(x => x.GetRequiredService<BiosInformationConverter>())
                    .AddSingleton<ISmbiosStructureWriter>(x => x.GetRequiredService<BiosInformationConverter>())
                    .AddSingleton<SystemInformationConverter>()
                    .AddSingleton<ISmbiosStructureReader>(x => x.GetRequiredService<SystemInformationConverter>())
                    .AddSingleton<ISmbiosStructureWriter>(x => x.GetRequiredService<SystemInformationConverter>())
                    .AddSingleton<SmbiosStructuresCommandHandlers>()
                ;
        }

        // known-structs
        {
            var command = new Command("known-structs", "List known structure types and it's status");
            parentCommand.Add(command);

            var outputOpt = new Option<string>("--output", "-o")
            {
                DefaultValueFactory = _ => "-"
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<SmbiosStructuresCommandHandlers>();
                handler.KnownStructs(
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }

        // extract-struct
        {
            var command = new Command("extract-struct", "Parse SMBIOS.json[--idx] structure to json");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Single json raw struct from table2json",
                Required = true,
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Structure in json",
                DefaultValueFactory = _ => "-"
            };
            command.Add(outputOpt);

            var handleOpt = new Option<int?>("--handle")
            {
                Description = "StructureHandle in smbios-table.json",
            };
            command.Add(handleOpt);

            var verifyOpt = new Option<bool>("--verify")
            {
                Description = "Verify that final json will be same after convert to raw section",
                DefaultValueFactory = _ => true,
            };
            command.Add(verifyOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<SmbiosStructuresCommandHandlers>();
                handler.ExtractStruct(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(handleOpt) ?? -1,
                    opts.GetRequiredValue(verifyOpt)
                );
            });
        }

        // inject-struct
        {
            var command = new Command("inject-struct", "Inject struct to SMBIOS.json by handler id");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Json representation of SMBIOS table input file",
                Required = true,
            };
            command.Add(inputOpt);

            var structOpt = new Option<string>("--struct", "-s")
            {
                Description = "Json representation of structure file",
                Required = true,
            };
            command.Add(structOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Json representation of SMBIOS table output file",
                DefaultValueFactory = _ => "-"
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<SmbiosStructuresCommandHandlers>();
                handler.Inject(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(structOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }
}
