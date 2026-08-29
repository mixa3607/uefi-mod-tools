namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public class FitTable
{
    public byte[] HeadGarbage { get; set; } = [];
    public List<FitEntry> Entries { get; set; } = [];
    public byte[] TailGarbage { get; set; } = [];
}
