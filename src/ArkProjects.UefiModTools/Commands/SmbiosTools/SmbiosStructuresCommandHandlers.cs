using ArkProjects.UefiModTools.Misc;
using ArkProjects.UefiModTools.Smbios;
using ArkProjects.UefiModTools.Smbios.Structures;
using ConsoleTables;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.SmbiosTools;

public class SmbiosStructuresCommandHandlers
{
    private readonly ILogger<SmbiosStructuresCommandHandlers> _logger;
    private readonly JsonSerializationService _jsonSerializer;
    private readonly IEnumerable<ISmbiosStructureReader> _readers;
    private readonly IEnumerable<ISmbiosStructureWriter> _writers;

    public SmbiosStructuresCommandHandlers(IEnumerable<ISmbiosStructureReader> readers,
        IEnumerable<ISmbiosStructureWriter> writers, ILogger<SmbiosStructuresCommandHandlers> logger,
        JsonSerializationService jsonSerializer)
    {
        _readers = readers;
        _writers = writers;
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    public int KnownStructs(string output)
    {
        var table = new ConsoleTable("Type", "Name", "Read", "Write");
        foreach (var t in Enum.GetValues<SmbiosStructureType>())
        {
            table.AddRow([
                (byte)t,
                t.ToString(),
                _readers.Any(x => x.AllowedStructureType == t),
                _writers.Any(x => x.AllowedStructureType == t),
            ]);
        }

        CommandHelpers.WriteResult(table.ToMarkDownString(), output, true, _logger);
        return 0;
    }

    public int ExtractStruct(string input, string output, int handle, bool verify)
    {
        _logger.LogInformation("Converting SMBIOS structure to json");
        var tableJson = File.ReadAllText(input);
        var table = _jsonSerializer.Deserialize<SmbiosDump>(tableJson);

        var structureRaw = table.Structures.FirstOrDefault(x => handle < 0 || x.StructureHandle == handle);
        if (structureRaw == null)
        {
            _logger.LogCritical("Structure with handle {handle} not found", handle);
            throw new Exception($"Structure with handle {handle} not found");
        }

        _logger.LogInformation("Found {name} structure with handle {handle}",
            structureRaw.StructureType, structureRaw.StructureHandle);

        var jsonDump = Extract(structureRaw, verify);

        CommandHelpers.WriteResult(jsonDump, output, true, _logger);
        return 0;
    }

    private string Extract(SmbiosRawStructure rawStructure, bool verify)
    {
        var reader = _readers.FirstOrDefault(x => x.AllowedStructureType == rawStructure.StructureType);
        if (reader == null)
        {
            _logger.LogCritical("{name} extractor not implemented", rawStructure.StructureType);
            throw new Exception($"{rawStructure.StructureType} extractor not implemented");
        }

        var structure = reader.Read(rawStructure);
        var structureJson = _jsonSerializer.Serialize(structure);
        if (!verify)
        {
            _logger.LogWarning("Skip repack verification!");
            return structureJson;
        }

        _logger.LogInformation("Verifying dump by repacking...");
        var writer = _writers.FirstOrDefault(x => x.AllowedStructureType == rawStructure.StructureType);
        if (writer == null)
        {
            _logger.LogWarning("Writer for {name} not implemented. Skip verification",
                rawStructure.StructureType);
            return structureJson;
        }

        structure = _jsonSerializer.Deserialize<ISmbiosStructure>(structureJson)!;
        var repackedRawStructure = writer.Write(structure);
        if (!rawStructure.Strings.ToArray().SequenceEqual(repackedRawStructure.Strings) ||
            !rawStructure.Body.ToArray().SequenceEqual(repackedRawStructure.Body) ||
            rawStructure.StructureType != repackedRawStructure.StructureType ||
            rawStructure.StructureHandle != repackedRawStructure.StructureHandle ||
            rawStructure.StructureLength != repackedRawStructure.StructureLength
           )
        {
            _logger.LogCritical("Repacked dump and source dump not equal!");
            throw new Exception("Repacked dump and source dump not equal!");
        }

        _logger.LogInformation("Repacking success! Old and new dumps will be equal");
        return structureJson;
    }

    public int Inject(string inputFile, string structureFile, string outputFile)
    {
        var tableJson = File.ReadAllText(inputFile);
        var table = _jsonSerializer.Deserialize<SmbiosDump>(tableJson)!;

        var structureJson = File.ReadAllText(structureFile);
        var structure = _jsonSerializer.Deserialize<ISmbiosStructure>(structureJson)!;

        var writer = _writers.FirstOrDefault(x => x.AllowedStructureType == structure.StructureType);
        if (writer == null)
        {
            _logger.LogCritical("{name} writer not implemented", structure.StructureType);
            throw new Exception($"{structure.StructureType} writer not implemented");
        }

        var rawStructure = writer.Write(structure);

        if (!TryReplace(table, rawStructure) && !TryInject(table, rawStructure))
        {
            _logger.LogCritical("Can not inject structure");
            throw new Exception("Can not inject structure");
        }


        tableJson = _jsonSerializer.Serialize(table);
        CommandHelpers.WriteResult(tableJson, outputFile, true, _logger);
        return 0;
    }

    private bool TryReplace(SmbiosDump table, SmbiosRawStructure rawStructure)
    {
        var replace = table.Structures.FirstOrDefault(x => x.StructureHandle == rawStructure.StructureHandle);
        if (replace == null)
            return false;

        _logger.LogInformation("Replacing struct {name} with handle {handle}",
            replace.StructureType, replace.StructureHandle);
        var idx = table.Structures.IndexOf(replace);
        table.Structures.RemoveAt(idx);
        table.Structures.Insert(idx, rawStructure);

        return true;
    }

    private bool TryInject(SmbiosDump table, SmbiosRawStructure rawStructure)
    {
        if (table.Structures.Count == 0)
        {
            _logger.LogInformation("No structures in table, just add");
            table.Structures.Add(rawStructure);
            return true;
        }

        var after = table.Structures
            .OrderBy(x => x.StructureHandle)
            .SkipWhile(x => x.StructureHandle < rawStructure.StructureHandle)
            .FirstOrDefault();
        if (after != null)
        {
            _logger.LogInformation("Placing struct after {name} with handle {handle}",
                after.StructureType, after.StructureHandle);
            var idx = table.Structures.IndexOf(after);
            table.Structures.Insert(idx, rawStructure);
            return true;
        }

        var before = table.Structures
            .OrderByDescending(x => x.StructureHandle)
            .SkipWhile(x => x.StructureHandle > rawStructure.StructureHandle)
            .FirstOrDefault();
        if (before != null)
        {
            _logger.LogInformation("Placing struct before {name} with handle {handle}",
                before.StructureType, before.StructureHandle);
            var idx = table.Structures.IndexOf(before);
            table.Structures.Insert(idx - 1, rawStructure);

            return true;
        }

        return false;
    }
}
