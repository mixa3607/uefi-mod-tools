namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;

public class Suppression
{
    public required string Offset { get; set; }
    public bool Active { get; set; }
    public required string Start { get; set; }
    public required string End { get; set; }
}
