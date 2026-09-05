using ArkProjects.UefiModTools.Services.ManifestVer;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;

public class SetupDataMapDocument : IVersionedManifest
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-SetupData-IFR-Map";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public required string SetupDataSha256 { get; set; }
    public required string IfrSha256 { get; set; }
    public List<SetupDataQuestionMapping> Questions { get; set; } = [];
}
