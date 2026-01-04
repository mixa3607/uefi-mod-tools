using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Smbios;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.SmbiosTools;

public class TableCommandHandlers
{
    private readonly ILogger<TableCommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly SmbiosReader _reader;
    private readonly SmbiosWriter _writer;
    private readonly ICommandFileManager _fileManager;

    public TableCommandHandlers(ILogger<TableCommandHandlers> logger, SmbiosReader reader,
        SmbiosWriter writer, IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _reader = reader;
        _writer = writer;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    public int Table2Json(string input, string output, bool verify)
    {
        _logger.LogInformation("Converting SMBIOS table to json dump");
        var origDumpBytes = File.ReadAllBytes(input);
        var jsonDump = Read(origDumpBytes, verify);

        _fileManager.Write(jsonDump, output, true);
        return 0;
    }

    private string Read(byte[] dumpBytes, bool verify)
    {
        using var dumpStream = new MemoryStream(dumpBytes);
        var dump = _reader.Read(dumpStream);
        var jsonDump = _jsonSerializer.Serialize(dump);

        if (!verify)
        {
            _logger.LogWarning("Skip repack verification!");
            return jsonDump;
        }

        _logger.LogInformation("Verifying dump by repacking...");
        dump = _jsonSerializer.Deserialize<SmbiosDump>(jsonDump);
        var repackStream = new MemoryStream();
        _writer.Write(dump, repackStream);
        if (repackStream.ToArray().SequenceEqual(dumpBytes))
        {
            _logger.LogInformation("Repacking success! Old and new dumps will be equal");
            return jsonDump;
        }

        _logger.LogCritical("Repacked dump and source dump not equal!");
        throw new Exception("Repacked dump and source dump not equal!");
    }

    public int Json2Table(string input, string output)
    {
        _logger.LogInformation("Converting SMBIOS json dump to table bin");
        var jsonDump = File.ReadAllText(input);
        var dump = _jsonSerializer.Deserialize<SmbiosDump>(jsonDump);
        var tableBytes = new byte[dump.Length];
        var tableStream = new MemoryStream(tableBytes);
        _writer.Write(dump, tableStream);
        tableStream.Flush();

        _fileManager.Write(tableBytes, output, true);
        return 0;
    }
}
