using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Commands.UefiTools.Sct.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Sct;

[JsonSerializable(typeof(SctPatchDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class SctJsonSerializerContext : JsonSerializerContext;
