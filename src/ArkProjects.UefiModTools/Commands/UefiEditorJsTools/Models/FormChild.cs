namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;

public class FormChild
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string QuestionId { get; set; }
    public string VarStoreId { get; set; }
    public string? VarStoreName { get; set; }
    public string? AccessLevel { get; set; }
    public string? Failsafe { get; set; }
    public string? Optimal { get; set; }
    public Offsets? Offsets { get; set; }
    public List<string>? SuppressIf { get; set; }
    public string Type { get; set; }

    /// <summary>
    /// "Ref"
    /// </summary>
    public string? FormId { get; set; }
}
