using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaults;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults.BiosDefaultsStore;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

[JsonSerializable(typeof(BiosDefaultsMapDocument))]
[JsonSerializable(typeof(BiosDefaultsStoreMapDocument))]
[JsonSerializable(typeof(BiosDefaultsStorePatchDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class CommandJsonSerializerContextIfrBiosDefaults : JsonSerializerContext;
