namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;

public class BiosDefaultsStoreMapDocument
{
    public const int SupportedVersion = 2;
    public const string SupportedType = "AF516361-BiosDefaults-Store-Map";

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
