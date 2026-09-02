using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

[JsonSerializable(typeof(SetupDataMapDocument))]
[JsonSerializable(typeof(SetupDataPatchDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class CommandJsonSerializerContextIfrSetupData : JsonSerializerContext;
