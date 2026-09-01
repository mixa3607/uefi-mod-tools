using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

[JsonSerializable(typeof(BiosDefaultsMapDocument))]
[JsonSerializable(typeof(BiosDefaultsStoreMapDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class CommandJsonSerializerContextIfrBiosDefaults : JsonSerializerContext;
