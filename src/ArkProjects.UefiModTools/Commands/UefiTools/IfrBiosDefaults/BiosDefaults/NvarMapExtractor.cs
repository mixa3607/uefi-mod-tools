using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaults;

public class NvarMapExtractor
{
    private readonly ILogger<NvarMapExtractor> _logger;

    public NvarMapExtractor(ILogger<NvarMapExtractor> logger)
    {
        _logger = logger;
    }

    public List<NvarVariableInfo> Extract(byte[] nvarData)
    {
        _logger.LogInformation("Parsing {size} bytes of BIOS defaults NVAR data", nvarData.Length);

        var variables = ReadVariables(nvarData, null);
        _logger.LogInformation("Extracted {count} NVAR variables", variables.Count);
        return variables;
    }

    private List<NvarVariableInfo> ReadVariables(byte[] nvarData, NvarVariableInfo? parentRecord)
    {
        using var stream = new MemoryStream(nvarData, writable: false);
        using var reader = new BinaryReader(stream);

        var startOffset = parentRecord?.DataOffset ?? 0;
        var endOffset = parentRecord is null
            ? nvarData.Length
            : parentRecord.RecordOffset + parentRecord.RecordSize;
        stream.Position = startOffset;

        var variables = new List<NvarVariableInfo>();
        while (stream.Position + 4 <= endOffset)
        {
            var recordOffset = checked((int)stream.Position);
            if (!reader.ReadBytes(4).SequenceEqual("NVAR"u8))
            {
                _logger.LogDebug("Stopped parsing NVAR records at offset 0x{offset:X}", recordOffset);
                break;
            }

            var recordSize = reader.ReadUInt16();
            const int minimumRecordSize = 0x0B;
            if (recordSize < minimumRecordSize)
                throw new InvalidDataException($"NVAR record at 0x{recordOffset:X} is too small: {recordSize} bytes");

            var recordEndOffset = checked(recordOffset + recordSize);
            if (recordEndOffset > endOffset)
            {
                throw new InvalidDataException(
                    $"NVAR record at 0x{recordOffset:X} extends past its containing record");
            }

            var next = reader.ReadUInt24();
            var attributes = (NvarAttributes)reader.ReadByte();
            reader.ReadByte();
            var name = reader.ReadNullTerminatedString(recordEndOffset);
            var dataOffset = checked((int)stream.Position);
            var dataLength = recordEndOffset - dataOffset;
            var data = reader.ReadBytes(dataLength);

            if (next != 0xFFFFFF)
            {
                throw new NotSupportedException($"Chained NVAR records are not supported. Next=0x{next:X6}");
            }

            var variable = new NvarVariableInfo
            {
                Name = name,
                RecordOffset = recordOffset,
                RecordSize = recordSize,
                DataOffset = dataOffset,
                ParentRecordOffset = parentRecord?.RecordOffset ?? -1,
                Attributes = attributes,
                Value = data,
            };
            variables.Add(variable);

            if (data.AsSpan().StartsWith("NVAR"u8))
                variables.AddRange(ReadVariables(nvarData, variable));
        }

        return variables;
    }
}
