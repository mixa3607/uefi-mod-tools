using ArkProjects.UefiModTools.Services.ManifestVer;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;

public class BiosDefaultsIfrMapDocument : IVersionedManifest
{
    public const int SupportedVersion = 2;
    public const string SupportedType = "BiosDefaults-Store-Map";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";

    public required string BiosDefaultsSha256 { get; set; }
    public required string IfrSha256 { get; set; }
    public List<BiosDefaultsQuestionMapping> QuestionMappings { get; set; } = [];
}

public class BiosDefaultsQuestionMapping
{
    public string? Id { get; set; }
    public ushort QuestionId { get; set; }
    public required string Opcode { get; set; }
    public required string VarStoreName { get; set; }
    public ushort VarStoreOffset { get; set; }
    public int? DataLength { get; set; }
    public BiosDefaultsMappingStatus Status { get; set; }
    public int? NvarDataOffset { get; set; }
    public string? Value { get; set; }
}

public enum BiosDefaultsMappingStatus
{
    Mapped,
    UnknownVarStore,
    MissingVarStoreSize,
    NvarSizeMismatch,
    AmbiguousNvarVariable,
    UnsupportedDataLength,
    NvarRangeExceeded,
}
