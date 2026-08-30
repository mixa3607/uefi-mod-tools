namespace ArkProjects.UefiModTools.Commands.UefiEditorJsTools.Models;


public class Data
{
    public required List<Menu> Menu { get; set; }
    public required List<VarStore> VarStores { get; set; }
    public required List<Form> Forms { get; set; }
    public required List<Suppression> Suppressions { get; set; }
    public required string Version { get; set; }
}
