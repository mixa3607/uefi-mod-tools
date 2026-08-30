using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

[JsonSerializable(typeof(ExtractedAmiSetupDataQuestions))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class CommandJsonSerializerContextIfrSetupData : JsonSerializerContext;
