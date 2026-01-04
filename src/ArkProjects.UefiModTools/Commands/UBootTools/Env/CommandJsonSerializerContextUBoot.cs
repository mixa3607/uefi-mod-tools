using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UBootTools.Env;

[JsonSerializable(typeof(UBootEnv))]
[JsonSerializable(typeof(UBootScanResult))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
internal partial class CommandJsonSerializerContextUBootEnv : JsonSerializerContext
{
}
