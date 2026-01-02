namespace ArkProjects.UefiModTools.Smbios.Structures;

public interface ISmbiosStructureWriter
{
    SmbiosStructureType AllowedStructureType { get; }
    SmbiosRawStructure Write(ISmbiosStructure body);
}
