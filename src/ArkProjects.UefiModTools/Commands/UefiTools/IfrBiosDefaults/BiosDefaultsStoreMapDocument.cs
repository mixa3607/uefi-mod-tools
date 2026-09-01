namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public class BiosDefaultsStoreMapDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AF516361-BiosDefaults-Store-Map";

    public int Version { get; set; } = SupportedVersion;
    public string Type { get; set; } = SupportedType;
    public required string BiosDefaultsSha256 { get; set; }
    public required string IfrSha256 { get; set; }
    public List<BiosDefaultsQuestionMapping> QuestionMappings { get; set; } = [];
}

public class BiosDefaultsQuestionMapping
{
    public ushort QuestionId { get; set; }
    public required string Opcode { get; set; }
    public required string VarStoreName { get; set; }
    public ushort VarStoreOffset { get; set; }
    public int? DataLength { get; set; }
    public BiosDefaultsMappingStatus Status { get; set; }
    public int? NvarDataOffset { get; set; }
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
