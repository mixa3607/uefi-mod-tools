using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

public class MicrocodesTable
{
    [JsonConverter(typeof(NumberConverterAsHex<uint>))]
    public uint SectionBaseAddress { get; set; } = 0;

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public int UsableStart { get; set; } = 0;

    [JsonConverter(typeof(NumberConverterAsHex<int>))]
    public int UsableEnd { get; set; } = -1;

    public required string[] MicrocodeFiles { get; set; }
}
