namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;

public class FormChild
{
    public required string Name { get; set; }
    public required string Description { get; set; }
    public required string QuestionId { get; set; }
    public required string VarStoreId { get; set; }
    public string? VarStoreName { get; set; }
    public string? AccessLevel { get; set; }
    public string? Failsafe { get; set; }
    public string? Optimal { get; set; }
    public Offsets? Offsets { get; set; }
    public List<string>? SuppressIf { get; set; }
    public required string Type { get; set; }

    /// <summary>
    /// "Ref"
    /// </summary>
    public string? FormId { get; set; }
}
