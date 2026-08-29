using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

[JsonSerializable(typeof(FitTable))]
[JsonSerializable(typeof(MicrocodesTable))]
internal partial class CommandJsonSerializerContextFit : JsonSerializerContext
{
}
