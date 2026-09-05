namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services.ManifestVer;

public class SctPatchDocument : IVersionedManifest
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-IFR-SCT-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<DisableSuppressIfPatch> SuppressIfPatches { get; set; } = [];
    public List<DefaultValuePatch> DefaultValuePatches { get; set; } = [];
    public List<OneOfOptionDefaultPatch> OneOfOptionDefaultPatches { get; set; } = [];
}

public class DisableSuppressIfPatch
{
    public bool Apply { get; set; }
    public int Offset { get; set; }
    public string? Comment { get; set; }
}

public class DefaultValuePatch
{
    public bool Apply { get; set; }

    public int Offset { get; set; }
    public required IfrTypeValue Value { get; set; }
    public string? Comment { get; set; }
}

public class OneOfOptionDefaultPatch
{
    public bool Apply { get; set; }
    public int Offset { get; set; }
    public bool Default { get; set; }
    public bool ManufacturingDefault { get; set; }
}
