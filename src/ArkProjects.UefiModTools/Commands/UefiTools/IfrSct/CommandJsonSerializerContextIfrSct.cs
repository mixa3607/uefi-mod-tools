using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSct;

[JsonSerializable(typeof(IfrSctPatches))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class CommandJsonSerializerContextIfrSct : JsonSerializerContext;
