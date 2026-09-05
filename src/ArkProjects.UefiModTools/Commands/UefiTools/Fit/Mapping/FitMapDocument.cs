using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using ArkProjects.UefiModTools.Services.ManifestVer;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;

public class FitMapDocument : IVersionedManifest
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "FIT-Map";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public required string FitSha256 { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public int TableOffset { get; set; }

    public List<FitEntryMapping> Entries { get; set; } = [];
}

public class FitEntryMapping
{
    public required string Id { get; set; }
    public int Index { get; set; }

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public int Offset { get; set; }

    public required FitEntry Entry { get; set; }
}
