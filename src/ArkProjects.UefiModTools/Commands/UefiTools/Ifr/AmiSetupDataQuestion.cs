using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

[StructLayout(LayoutKind.Explicit, Pack = 1, Size = Size)]
public struct AmiSetupDataQuestion
{
    public const int Size = 0x36;

    [FieldOffset(AmiSetupDataQuestionOffset.QuestionId)]
    [JsonInclude]
    [JsonPropertyName("questionId")]
    public ushort QuestionId;

    [FieldOffset(AmiSetupDataQuestionOffset.PageId)]
    [JsonInclude]
    [JsonPropertyName("pageId")]
    public ushort PageId;

    [FieldOffset(AmiSetupDataQuestionOffset.AccessLevel)]
    [JsonInclude]
    [JsonPropertyName("accessLevel")]
    public byte AccessLevel;

    [FieldOffset(AmiSetupDataQuestionOffset.HelpStringId)]
    [JsonInclude]
    [JsonPropertyName("helpStringId")]
    public ushort HelpStringId;

    [FieldOffset(AmiSetupDataQuestionOffset.PromptStringId)]
    [JsonInclude]
    [JsonPropertyName("promptStringId")]
    public ushort PromptStringId;

    [FieldOffset(AmiSetupDataQuestionOffset.Failsafe)]
    [JsonInclude]
    [JsonPropertyName("failsafe")]
    public byte Failsafe;

    [FieldOffset(AmiSetupDataQuestionOffset.Optimal)]
    [JsonInclude]
    [JsonPropertyName("optimal")]
    public byte Optimal;
}
