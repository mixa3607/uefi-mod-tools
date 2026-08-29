using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class FmhSectionModel : IFmhSectionModel
{
    public const string SectionType = "FMH";

    [JsonIgnore]
    public string Type => SectionType;

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int EndAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int ModuleBeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public required int ModuleEndAddress { get; set; }

    public required string ModuleName { get; set; }
}
