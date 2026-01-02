using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Misc;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public class FmhSectionModel : IFmhSectionModel
{
    public string Type => "FMH";

    [JsonConverter(typeof(HexConverter))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int EndAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int ModuleBeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    public required int ModuleEndAddress { get; set; }

    public required string ModuleName { get; set; }
}
