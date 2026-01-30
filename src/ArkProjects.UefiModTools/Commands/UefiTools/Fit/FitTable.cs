namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class FitTable
{
    public byte[] HeadGarbage { get; set; } = [];
    public List<FitEntry> Entries { get; set; } = [];
    public byte[] TailGarbage { get; set; } = [];
}