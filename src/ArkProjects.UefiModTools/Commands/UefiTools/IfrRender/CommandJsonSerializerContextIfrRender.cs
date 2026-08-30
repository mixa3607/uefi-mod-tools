using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(IfrJsonDocument))]
[JsonSerializable(typeof(IfrRenderDocument))]
public partial class CommandJsonSerializerContextIfrRender : JsonSerializerContext;
