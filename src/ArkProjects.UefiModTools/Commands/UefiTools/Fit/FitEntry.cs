using ArkProjects.UefiModTools.Utils;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class FitEntry
{
    /// <summary>
    /// 7:0 Address
    /// </summary>
    [JsonConverter(typeof(NumberConverterAsHex<ulong>))]
    public ulong Address { get; set; }

    /// <summary>
    /// 10:8 Size
    /// </summary>
    [JsonConverter(typeof(NumberConverterAsHex<uint>))]
    public uint Size { get; set; }

    /// <summary>
    /// 11 Reserved
    /// </summary>
    public byte Reserved { get; set; }

    /// <summary>
    /// 13:12 Version
    /// </summary>
    [JsonConverter(typeof(NumberConverterAsHex<ushort>))]
    public ushort Version { get; set; }

    /// <summary>
    /// 14 Bit 7 - C_V
    /// </summary>
    public bool ChecksumValidate { get; set; }

    /// <summary>
    /// 14 Bits 6:0 - Type
    /// </summary>
    public FitEntryType Type { get; set; }

    /// <summary>
    /// 15 Chksum
    /// </summary>
    public byte Checksum { get; set; }
}
