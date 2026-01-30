using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
[JsonSerializable(typeof(FitTable))]
[JsonSerializable(typeof(MicrocodesTable))]
internal partial class CommandJsonSerializerContextUefiTools : JsonSerializerContext
{
}
