using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrStringReference
{
    [JsonPropertyName("id")]
    public ushort Id { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
