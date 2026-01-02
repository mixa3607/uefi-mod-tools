using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Misc;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

public interface IFmhSectionModel
{
    string Type { get; }

    [JsonConverter(typeof(HexConverter))]
    int BeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    int EndAddress { get; set; }

    long Length => EndAddress - BeginAddress;
}
