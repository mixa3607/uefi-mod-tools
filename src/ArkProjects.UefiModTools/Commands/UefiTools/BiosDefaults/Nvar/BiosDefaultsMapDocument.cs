using ArkProjects.UefiModTools.Services.ManifestVer;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Nvar;

public class BiosDefaultsMapDocument : IVersionedManifest
{
    public const int SupportedVersion = 2;
    public const string SupportedType = "BiosDefaults-Map";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";

    public required string SourceSha256 { get; set; }
    public required string SourceName { get; set; }
    public required List<NvarVariableInfo> Variables { get; set; }
}

public class NvarVariableInfo
{
    public int ParentRecordOffset { get; set; } = -1;

    public required string Name { get; set; }
    public int RecordOffset { get; set; }
    public int RecordSize { get; set; }

    public NvarAttributes Attributes { get; set; }

    public int DataOffset { get; set; }
    public int DataLength => RecordOffset + RecordSize - DataOffset;
    public byte[] Value { get; set; } = [];
}

[Flags]
public enum NvarAttributes : byte
{
    None = 0,
    RuntimeVariable = 0x01,
    AsciiName = 0x02,
    LocalGuid = 0x04,
    DataOnly = 0x08,
}
