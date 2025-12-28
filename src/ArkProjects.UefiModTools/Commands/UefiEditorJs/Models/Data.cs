namespace ArkProjects.UefiModTools.Commands.UefiEditorJs.Models;


public class Data
{
    public List<Menu> Menu { get; set; }
    public List<VarStore> VarStores { get; set; }
    public List<Form> Forms { get; set; }
    public List<Suppression> Suppressions { get; set; }
    public string Version { get; set; }
}