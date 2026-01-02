namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools;

public class BiosSection
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public List<BiosSection>? Childs { get; set; }
    public List<BiosSectionSuppressIf>? SuppressIf { get; set; }
}
