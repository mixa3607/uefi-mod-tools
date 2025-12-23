namespace ArkProjects.UefiModTools;

public class SmbiosDump
{
    public int Length { get; set; } = 0;
    public List<SmbiosRawStructure> Structures { get; set; } = [];
}
