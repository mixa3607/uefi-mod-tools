namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

using ArkProjects.UefiModTools.Ifr.Structures;

public class SctPatchDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-IFR-SCT-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<DisableSuppressIfPatch> SuppressIfPatches { get; set; } = [];
    public List<DefaultValuePatch> DefaultValuePatches { get; set; } = [];
}

public class DisableSuppressIfPatch
{
    public bool Apply { get; set; }
    public int Offset { get; set; }
}

public class DefaultValuePatch
{
    public bool Apply { get; set; }

    public int Offset { get; set; }
    public required IfrTypeValue Value { get; set; }
}
