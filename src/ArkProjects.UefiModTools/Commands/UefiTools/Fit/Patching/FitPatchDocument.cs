using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Services.ManifestVer;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;

public class FitPatchDocument : IVersionedManifest
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "FIT-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<FitPatchOperation> Operations { get; set; } = [];
}

public class FitPatchOperation
{
    public FitPatchOperationKind Kind { get; set; }
    public required string Id { get; set; }
    public FitEntry? Entry { get; set; }
}

public enum FitPatchOperationKind
{
    Clear,
    Write,
}
