namespace ArkProjects.UefiModTools.Smbios;

public class SmbiosRawStructure
{
    public SmbiosStructureType StructureType { get; set; }
    public byte StructureLength => (byte)(Body.Length + sizeof(byte) + sizeof(byte) + sizeof(ushort));
    public ushort StructureHandle { get; set; }

    public string[] Strings { get; set; } = [];
    public byte[] Body { get; set; } = [];
}
