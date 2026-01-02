using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Misc;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class FmhTailSectionModel : IFmhSectionModel
{
    public string Type => "FMH-end";

    [JsonConverter(typeof(HexConverter))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int EndAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int PointingToAddress { get; set; }
}
