namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

public class SetupDataMapDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-SetupData-IFR-Map";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public required string SetupDataSha256 { get; set; }
    public required string IfrSha256 { get; set; }
    public List<ExtractedAmiSetupDataQuestion> Questions { get; set; } = [];
}
