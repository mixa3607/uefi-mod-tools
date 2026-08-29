using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrTypeValue
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    [JsonPropertyName("hour")]
    public byte? Hour { get; set; }

    [JsonPropertyName("minute")]
    public byte? Minute { get; set; }

    [JsonPropertyName("second")]
    public byte? Second { get; set; }

    [JsonPropertyName("year")]
    public ushort? Year { get; set; }

    [JsonPropertyName("month")]
    public byte? Month { get; set; }

    [JsonPropertyName("day")]
    public byte? Day { get; set; }

    [JsonPropertyName("string")]
    public IfrStringReference? String { get; set; }

    [JsonPropertyName("hex")]
    public string? Hex { get; set; }

    [JsonPropertyName("question_id")]
    public ushort? QuestionId { get; set; }

    [JsonPropertyName("form_id")]
    public ushort? FormId { get; set; }

    [JsonPropertyName("formset_guid")]
    public string? FormsetGuid { get; set; }

    [JsonPropertyName("device_path_string_id")]
    public ushort? DevicePathStringId { get; set; }
}
