using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.BinTools.Models;

public class Partition
{
    public required string FileName { get; set; }
    public string? Description { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int EndAddress { get; set; }

    public byte PadByte { get; set; } = 0xff;
    public long Length => EndAddress - BeginAddress;
}
