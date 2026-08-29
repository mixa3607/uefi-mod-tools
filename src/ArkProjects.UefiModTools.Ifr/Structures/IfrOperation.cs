using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrOperation
{
    [JsonPropertyName("offset")]
    public ulong Offset { get; set; }

    [JsonPropertyName("opcode")]
    public string Opcode { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public byte Length { get; set; }

    [JsonPropertyName("scope_start")]
    public bool ScopeStart { get; set; }

    [JsonPropertyName("depth")]
    public ulong Depth { get; set; }

    [JsonPropertyName("raw_hex")]
    public string? RawHex { get; set; }

    [JsonPropertyName("fields")]
    public IfrOperationFields Fields { get; set; } = new();
}
