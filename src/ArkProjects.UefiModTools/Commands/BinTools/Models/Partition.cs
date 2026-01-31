using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.BinTools.Models;

public class Partition
{
    public required string FileName { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int EndAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<byte>))]
    public byte PadByte { get; set; } = 0xff;

    public long Length => EndAddress - BeginAddress;
}
