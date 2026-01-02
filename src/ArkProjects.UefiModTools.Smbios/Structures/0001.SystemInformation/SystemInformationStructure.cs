using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Smbios.Structures;

public class SystemInformationStructure : ISmbiosStructure
{
    [JsonIgnore]
    public SmbiosStructureType StructureType => SmbiosStructureType.SystemInformation;
    public ushort StructureHandle { get; set; }

    public required string Manufacturer { get; set; }
    public required string ProductName { get; set; }
    public required string Version { get; set; }
    public required string SerialNumber { get; set; }
    public required Guid Uuid { get; set; }
    public required SystemWakeupType WakeUpType { get; set; }
    public required string SkuNumber { get; set; }
    public required string Family { get; set; }

    //public override bool Equals(object? obj)
    //{
    //    if (obj is SystemInformationStructure s)
    //    {
    //        if (StructureHandle != s.StructureHandle)
    //            return false;
    //        if (Manufacturer != s.Manufacturer)
    //            return false;
    //        if (ProductName != s.ProductName)
    //            return false;
    //        if (Version != s.Version)
    //            return false;
    //        if (SerialNumber != s.SerialNumber)
    //            return false;
    //        if (Uuid != s.Uuid)
    //            return false;
    //        if (WakeUpType != s.WakeUpType)
    //            return false;
    //        if (SkuNumber != s.SkuNumber)
    //            return false;
    //        if (Family != s.Family)
    //            return false;
    //    }
    //    return base.Equals(obj);
    //}
}
