using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

[JsonSerializable(typeof(UBootEnv))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
internal partial class CommandJsonSerializerContextUBoot : JsonSerializerContext
{
}
