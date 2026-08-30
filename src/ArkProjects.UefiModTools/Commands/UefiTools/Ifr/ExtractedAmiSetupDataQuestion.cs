using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public class ExtractedAmiSetupDataQuestion
{
    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int EndAddress { get; set; }

    public required string Type { get; set; }

    public required AmiSetupDataQuestion Question { get; set; }
}
