namespace ArkProjects.UefiModTools.Smbios;

public class SmbiosDump
{
    public int Length { get; set; } = 0;
    public List<SmbiosRawStructure> Structures { get; set; } = [];
}
