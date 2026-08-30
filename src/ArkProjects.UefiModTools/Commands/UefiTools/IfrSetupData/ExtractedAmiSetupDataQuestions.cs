namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

public class ExtractedAmiSetupDataQuestions
{
    public int Version { get; set; } = 1;
    public List<ExtractedAmiSetupDataQuestion> Questions { get; set; } = [];
}
