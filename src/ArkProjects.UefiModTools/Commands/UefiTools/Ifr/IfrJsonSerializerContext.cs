using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(IfrJsonDocument))]
[JsonSerializable(typeof(IfrDocument))]
public partial class IfrJsonSerializerContext : JsonSerializerContext;
