using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

[JsonSerializable(typeof(BiosDefaultsMapDocument))]
public partial class CommandJsonSerializerContextIfrBiosDefaults : JsonSerializerContext;
