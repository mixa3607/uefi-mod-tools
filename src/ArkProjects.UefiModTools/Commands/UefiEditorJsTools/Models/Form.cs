namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;

public class Form
{
    public required string Name { get; set; }
    public string Type { get; set; } = "Form";
    public required string FormId { get; set; }
    public required List<string> ReferencedIn { get; set; }
    public required List<FormChild> Children { get; set; }
}
