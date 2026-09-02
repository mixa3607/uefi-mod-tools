namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Patching;

public class SetupDataPatchDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-SetupData-Patch";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public List<SetupDataQuestionPatch> Questions { get; set; } = [];
}

public class SetupDataQuestionPatch
{
    public required string Id { get; set; }
    public byte? AccessLevel { get; set; }
    public byte? Failsafe { get; set; }
    public byte? Optimal { get; set; }
}
