using System.Text.Json;
using ArkProjects.UefiModTools.Smbios;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.Smbios;

public class SmbiosTableCommandHandlers
{
    private readonly ILogger<SmbiosTableCommandHandlers> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly SmbiosReader _reader;
    private readonly SmbiosWriter _writer;

    public SmbiosTableCommandHandlers(ILogger<SmbiosTableCommandHandlers> logger, SmbiosReader reader,
        SmbiosWriter writer, JsonSerializerOptions jsonSerializerOptions)
    {
        _logger = logger;
        _reader = reader;
        _writer = writer;
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public int Table2Json(string input, string output, bool verify)
    {
        _logger.LogInformation("Converting SMBIOS table to json dump");
        var origDumpBytes = File.ReadAllBytes(input);
        var jsonDump = Read(origDumpBytes, verify);

        CommandHelpers.WriteResult(jsonDump, output, true, _logger);
        return 0;
    }

    private string Read(byte[] dumpBytes, bool verify)
    {
        using var dumpStream = new MemoryStream(dumpBytes);
        var dump = _reader.Read(dumpStream);
        var jsonDump = JsonSerializer.Serialize(dump, _jsonSerializerOptions);

        if (!verify)
        {
            _logger.LogWarning("Skip repack verification!");
            return jsonDump;
        }

        _logger.LogInformation("Verifying dump by repacking...");
        dump = JsonSerializer.Deserialize<SmbiosDump>(jsonDump, _jsonSerializerOptions)!;
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
        var dump = JsonSerializer.Deserialize<SmbiosDump>(jsonDump, _jsonSerializerOptions)!;
        var tableBytes = new byte[dump.Length];
        var tableStream = new MemoryStream(tableBytes);
        _writer.Write(dump, tableStream);
        tableStream.Flush();

        CommandHelpers.WriteResult(tableBytes, output, true, _logger);
        return 0;
    }
}
