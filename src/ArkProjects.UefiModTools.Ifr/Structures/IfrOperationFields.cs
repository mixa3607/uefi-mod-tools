using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrOperationFields
{
    [JsonPropertyName("parse_error")]
    public string? ParseError { get; set; }

    [JsonPropertyName("kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("form_id")]
    public ushort? FormId { get; set; }

    [JsonPropertyName("title")]
    public IfrStringReference? Title { get; set; }

    [JsonPropertyName("prompt")]
    public IfrStringReference? Prompt { get; set; }

    [JsonPropertyName("help")]
    public IfrStringReference? Help { get; set; }

    [JsonPropertyName("text")]
    public IfrStringReference? Text { get; set; }

    [JsonPropertyName("question_id")]
    public ushort? QuestionId { get; set; }

    [JsonPropertyName("other_question_id")]
    public ushort? OtherQuestionId { get; set; }

    [JsonPropertyName("ref_question_id")]
    public ushort? RefQuestionId { get; set; }

    [JsonPropertyName("varstore_id")]
    public ushort? VarStoreId { get; set; }

    [JsonPropertyName("var_offset")]
    public ushort? VarOffset { get; set; }

    [JsonPropertyName("question_flags")]
    public byte? QuestionFlags { get; set; }

    [JsonPropertyName("flags")]
    public byte? Flags { get; set; }

    [JsonPropertyName("min_max_step")]
    public IfrMinMaxStep? MinMaxStep { get; set; }

    [JsonPropertyName("min_size")]
    public byte? MinSize { get; set; }

    [JsonPropertyName("max_size")]
    public byte? MaxSize { get; set; }

    [JsonPropertyName("max_containers")]
    public byte? MaxContainers { get; set; }

    [JsonPropertyName("default")]
    public bool? Default { get; set; }

    [JsonPropertyName("mfg_default")]
    public bool? MfgDefault { get; set; }

    [JsonPropertyName("option")]
    public IfrStringReference? Option { get; set; }

    [JsonPropertyName("default_id")]
    public ushort? DefaultId { get; set; }

    [JsonPropertyName("value")]
    public JsonElement? Value { get; set; }

    [JsonPropertyName("name")]
    public JsonElement? Name { get; set; }

    [JsonPropertyName("guid")]
    public string? Guid { get; set; }

    [JsonPropertyName("class_guids")]
    public List<string>? ClassGuids { get; set; }

    [JsonPropertyName("formset_guid")]
    public string? FormsetGuid { get; set; }

    [JsonPropertyName("device_path_id")]
    public ushort? DevicePathId { get; set; }

    [JsonPropertyName("size")]
    public ushort? Size { get; set; }

    [JsonPropertyName("attributes")]
    public uint? Attributes { get; set; }

    [JsonPropertyName("values")]
    public List<ushort>? Values { get; set; }
}
