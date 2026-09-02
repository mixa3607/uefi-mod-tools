namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;

public class BiosDefaultsStorePatchDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AF516361-BiosDefaults-Store-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<BiosDefaultsStoreValuePatch> VarPatches { get; set; } = [];
}

public class BiosDefaultsStoreValuePatch
{
    public required string Id { get; set; }
    public required string Value { get; set; }
}
