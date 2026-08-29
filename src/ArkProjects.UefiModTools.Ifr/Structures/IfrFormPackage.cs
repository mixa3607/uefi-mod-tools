using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrFormPackage
{
    [JsonPropertyName("index")]
    public ulong Index { get; set; }

    [JsonPropertyName("offset")]
    public ulong Offset { get; set; }

    [JsonPropertyName("length")]
    public ulong Length { get; set; }

    [JsonPropertyName("used_strings")]
    public ulong UsedStrings { get; set; }

    [JsonPropertyName("min_string_id")]
    public ushort MinStringId { get; set; }

    [JsonPropertyName("max_string_id")]
    public ushort MaxStringId { get; set; }
}
