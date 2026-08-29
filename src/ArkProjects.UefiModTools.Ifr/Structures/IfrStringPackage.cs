using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrStringPackage
{
    [JsonPropertyName("index")]
    public ulong Index { get; set; }

    [JsonPropertyName("offset")]
    public ulong Offset { get; set; }

    [JsonPropertyName("length")]
    public ulong Length { get; set; }

    [JsonPropertyName("language")]
    public string Language { get; set; } = string.Empty;

    [JsonPropertyName("total_strings")]
    public ulong TotalStrings { get; set; }

    [JsonPropertyName("coverage")]
    public ulong Coverage { get; set; }

    [JsonPropertyName("unresolved_strings")]
    public ulong UnresolvedStrings { get; set; }
}
