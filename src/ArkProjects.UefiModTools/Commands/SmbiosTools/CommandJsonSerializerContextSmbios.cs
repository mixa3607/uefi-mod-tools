using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Smbios;
using ArkProjects.UefiModTools.Smbios.Structures;

namespace ArkProjects.UefiModTools.Commands.SmbiosTools;

[JsonSerializable(typeof(ISmbiosStructure))]
[JsonSerializable(typeof(SmbiosDump))]
[JsonSerializable(typeof(SmbiosRawStructure))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
internal partial class CommandJsonSerializerContextSmbios : JsonSerializerContext
{
}
