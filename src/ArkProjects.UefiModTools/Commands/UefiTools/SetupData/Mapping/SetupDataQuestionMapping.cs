using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Format;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;

public class SetupDataQuestionMapping
{
    public required string Id { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int EndAddress { get; set; }

    public required string Type { get; set; }

    public required AmiSetupDataQuestion Question { get; set; }
}
