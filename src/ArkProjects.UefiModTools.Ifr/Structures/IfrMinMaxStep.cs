using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrMinMaxStep
{
    [JsonPropertyName("size_bits")]
    public byte SizeBits { get; set; }

    [JsonPropertyName("min")]
    public ulong? Min { get; set; }

    [JsonPropertyName("max")]
    public ulong? Max { get; set; }

    [JsonPropertyName("step")]
    public ulong? Step { get; set; }
}
