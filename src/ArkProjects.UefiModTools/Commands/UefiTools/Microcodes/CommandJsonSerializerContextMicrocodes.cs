using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

[JsonSerializable(typeof(MicrocodesTable))]
internal partial class CommandJsonSerializerContextMicrocodes : JsonSerializerContext
{
}
