using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Smbios.Structures;

[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(StructureType))]
[JsonDerivedType(typeof(BiosInformationStructure), typeDiscriminator: nameof(SmbiosStructureType.BiosInformation))]
[JsonDerivedType(typeof(SystemInformationStructure), typeDiscriminator: nameof(SmbiosStructureType.SystemInformation))]
public interface ISmbiosStructure
{
    [JsonIgnore]
    public SmbiosStructureType StructureType { get; }
    public ushort StructureHandle { get; set; }
}