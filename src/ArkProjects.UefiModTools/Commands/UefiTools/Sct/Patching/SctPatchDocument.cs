namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

public class SctPatchDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-IFR-SCT-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<DisableSuppressIfPatch> SuppressIfPatches { get; set; } = [];
}

public class DisableSuppressIfPatch
{
    public bool Disable { get; set; }
    public int Offset { get; set; }
}
