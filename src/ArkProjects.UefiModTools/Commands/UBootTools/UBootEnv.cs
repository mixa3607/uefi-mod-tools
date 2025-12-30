namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class UBootEnv
{
    public int Size { get; set; }
    public int PaddingSize { get; set; }
    public uint Hash { get; set; }
    public Dictionary<string, string> Variables { get; set; } = [];
}