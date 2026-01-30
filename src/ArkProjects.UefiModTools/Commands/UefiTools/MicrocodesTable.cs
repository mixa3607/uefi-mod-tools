using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class MicrocodesTable
{
    [JsonConverter(typeof(NumberConverter<uint>))]
    public uint SectionBaseAddress { get; set; } = 0;

    [JsonConverter(typeof(NumberConverter<int>))]
    public required int UsableStart { get; set; }

    [JsonConverter(typeof(NumberConverter<int>))]
    public required int UsableEnd { get; set; } = -1;

    public required string[] MicrocodeFiles { get; set; }
}
