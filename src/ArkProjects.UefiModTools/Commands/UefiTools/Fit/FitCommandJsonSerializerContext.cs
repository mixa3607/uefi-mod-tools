using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

[JsonSerializable(typeof(FitMapDocument))]
[JsonSerializable(typeof(FitPatchDocument))]
internal partial class FitCommandJsonSerializerContext : JsonSerializerContext
{
}
