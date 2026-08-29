using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class FmhTailSectionModel : IFmhSectionModel
{
    public const string SectionType = "FMH-end";

    [JsonIgnore]
    public string Type => SectionType;

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int EndAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int PointingToAddress { get; set; }
}
