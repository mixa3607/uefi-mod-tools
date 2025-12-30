namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;

public class Form
{
    public string Name { get; set; }
    public string Type { get; set; } = "Form";
    public string FormId { get; set; }
    public List<string> ReferencedIn { get; set; }
    public List<FormChild> Children { get; set; }
}