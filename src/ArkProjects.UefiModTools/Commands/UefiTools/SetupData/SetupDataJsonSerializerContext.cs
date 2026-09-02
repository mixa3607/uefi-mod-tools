using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Patching;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData;

[JsonSerializable(typeof(SetupDataMapDocument))]
[JsonSerializable(typeof(SetupDataPatchDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class SetupDataJsonSerializerContext : JsonSerializerContext;
