namespace ArkProjects.UefiModTools.Smbios.Structures;

public interface ISmbiosStructureReader
{
    SmbiosStructureType AllowedStructureType { get; }
    ISmbiosStructure Read(SmbiosRawStructure structure);
}