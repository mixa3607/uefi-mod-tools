using System.CommandLine;
using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Misc;
using ArkProjects.UefiModTools.Smbios;
using ArkProjects.UefiModTools.Smbios.Structures;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.SmbiosTools;

public class CommandRegistration
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
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextSmbios.Default)
            .AddSingleton<SmbiosReader>()
            .AddSingleton<SmbiosWriter>()
            .AddSingleton<TableCommandHandlers>()
            ;

        // unpack
        {
            var command = parentCommand.AddCommand("table2json", "Parse SMBIOS table to RAW structures");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description =
                        "Dump of 'File:SMbiosStaticData[DAF4BF89-CE71-4917-B522-C89D32FBC59F]' => 'Section:[*]' body",
                    Required = true,
                });
            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Json representation output file",
                    DefaultValueFactory = _ => "smbios-table.json",
                });
            var verifyOpt = command.AddOption(
                new Option<bool>("--verify")
                {
                    Description = "Verify that final json will be same after convert to table",
                    DefaultValueFactory = _ => true,
                });

            command.SetAction<TableCommandHandlers>(services,
                (handler, opts) => handler.Table2Json(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(verifyOpt)
                ));
        }

        // pack
        {
            var command = parentCommand.AddCommand("json2table", "Convert json dump to SMBIOS table bin");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Json representation of SMBIOS table input file",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description =
                        "Binary for 'File:SMbiosStaticData[DAF4BF89-CE71-4917-B522-C89D32FBC59F]' => 'Section:[*]' body",
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<TableCommandHandlers>(services,
                (handler, opts) => handler.Json2Table(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }

    private static void RegisterStructureCommands(Command parentCommand, IServiceCollection services)
    {
        services
            .AddSingleton<IJsonTypeInfoResolver>(CommandJsonSerializerContextSmbios.Default)
            .AddSingleton<BiosInformationConverter>()
            .AddSingleton<ISmbiosStructureReader>(x => x.GetRequiredService<BiosInformationConverter>())
            .AddSingleton<ISmbiosStructureWriter>(x => x.GetRequiredService<BiosInformationConverter>())
            .AddSingleton<SystemInformationConverter>()
            .AddSingleton<ISmbiosStructureReader>(x => x.GetRequiredService<SystemInformationConverter>())
            .AddSingleton<ISmbiosStructureWriter>(x => x.GetRequiredService<SystemInformationConverter>())
            .AddSingleton<StructuresCommandHandlers>()
            ;

        // known-structs
        {
            var command = parentCommand.AddCommand("known-structs", "List known structure types and it's status");

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<StructuresCommandHandlers>(services,
                (handler, opts) => handler.KnownStructs(
                    opts.GetRequiredValue(outputOpt)
                ));
        }

        // extract-struct
        {
            var command = parentCommand.AddCommand("extract-struct", "Parse SMBIOS.json[--idx] structure to json");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Single json raw struct from table2json",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Structure in json",
                    DefaultValueFactory = _ => "-"
                });

            var handleOpt = command.AddOption(
                new Option<int?>("--handle")
                {
                    Description = "StructureHandle in smbios-table.json",
                });

            var verifyOpt = command.AddOption(
                new Option<bool>("--verify")
                {
                    Description = "Verify that final json will be same after convert to raw section",
                    DefaultValueFactory = _ => true,
                });

            command.SetAction<StructuresCommandHandlers>(services,
                (handler, opts) => handler.ExtractStruct(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt),
                    opts.GetRequiredValue(handleOpt) ?? -1,
                    opts.GetRequiredValue(verifyOpt)
                ));
        }

        // inject-struct
        {
            var command = parentCommand.AddCommand("inject-struct", "Inject struct to SMBIOS.json by handler id");

            var inputOpt = command.AddOption(
                new Option<string>("--input", "-i")
                {
                    Description = "Json representation of SMBIOS table input file",
                    Required = true,
                });

            var structOpt = command.AddOption(
                new Option<string>("--struct", "-s")
                {
                    Description = "Json representation of structure file",
                    Required = true,
                });

            var outputOpt = command.AddOption(
                new Option<string>("--output", "-o")
                {
                    Description = "Json representation of SMBIOS table output file",
                    DefaultValueFactory = _ => "-"
                });

            command.SetAction<StructuresCommandHandlers>(services,
                (handler, opts) => handler.Inject(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(structOpt),
                    opts.GetRequiredValue(outputOpt)
                ));
        }
    }
}
