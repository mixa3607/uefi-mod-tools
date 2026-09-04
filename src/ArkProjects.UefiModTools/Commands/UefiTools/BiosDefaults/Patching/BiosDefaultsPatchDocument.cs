namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;

public class BiosDefaultsPatchDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "BiosDefaults-Store-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<BiosDefaultsValuePatch> VarPatches { get; set; } = [];
}

public class BiosDefaultsValuePatch
{
    public required string Id { get; set; }
    public required string Value { get; set; }
    public string? Comment { get; set; }
}
