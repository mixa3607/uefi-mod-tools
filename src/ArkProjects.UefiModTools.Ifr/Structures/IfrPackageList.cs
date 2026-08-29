using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrPackageList
{
    [JsonPropertyName("index")]
    public ulong Index { get; set; }

    [JsonPropertyName("offset")]
    public ulong Offset { get; set; }

    [JsonPropertyName("length")]
    public ulong Length { get; set; }

    [JsonPropertyName("guid")]
    public string Guid { get; set; } = string.Empty;
}
