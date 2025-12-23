using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiEditorJs;


public class Data
{
    public List<Menu> Menu { get; set; }
    public List<VarStore> VarStores { get; set; }
    public List<Form> Forms { get; set; }
    public List<Suppression> Suppressions { get; set; }
    public string Version { get; set; }
}

public class Suppression
{
    public string Offset { get; set; }
    public bool Active { get; set; }
    public string Start { get; set; }
    public string End { get; set; }
}

public class Menu
{
    public string Name { get; set; }
    public string FormId { get; set; }
    public string Offset { get; set; }
}

public class VarStore
{
    public string VarStoreId { get; set; }
    public string Size { get; set; }
    public string Name { get; set; }
}

public class Form
{
    public string Name { get; set; }
    public string Type { get; set; } = "Form";
    public string FormId { get; set; }
    public List<string> ReferencedIn { get; set; }
    public List<FormChild> Children { get; set; }
}

public class Offsets
{
    public string AccessLevel { get; set; }
    public string Failsafe { get; set; }
    public string Optimal { get; set; }
    public string? PageId { get; set; }
}


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
