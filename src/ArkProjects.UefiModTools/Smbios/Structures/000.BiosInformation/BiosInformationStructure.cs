using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Smbios.Structures;

public class BiosInformationStructure : ISmbiosStructure
{
    [JsonIgnore]
    public SmbiosStructureType StructureType => SmbiosStructureType.SystemInformation;
    public ushort StructureHandle { get; set; }
    /// <summary>
    /// Vendor
    /// </summary>
    public required string Vendor { get; set; }

    public required string Version { get; set; }
    public required ushort StartingAddressSegment { get; set; }
    public required string ReleaseDate { get; set; }
    public required ulong RomSize { get; set; }
    public required ulong Characteristics { get; set; }
    public required byte[] CharacteristicsExtensions { get; set; }
    public required byte SystemBiosMajorRelease { get; set; }
    public required byte SystemBiosMinorRelease { get; set; }
    public required byte EmbeddedControllerFirmwareMajorRelease { get; set; }
    public required byte EmbeddedControllerFirmwareMinorRelease { get; set; }
}
