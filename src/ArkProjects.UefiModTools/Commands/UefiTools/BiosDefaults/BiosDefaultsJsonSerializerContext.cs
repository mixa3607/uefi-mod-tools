using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.IfrMapping;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Nvar;
using ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults.Patching;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.BiosDefaults;

[JsonSerializable(typeof(BiosDefaultsMapDocument))]
[JsonSerializable(typeof(BiosDefaultsIfrMapDocument))]
[JsonSerializable(typeof(BiosDefaultsPatchDocument))]
[JsonSerializable(typeof(IfrJsonDocument))]
public partial class BiosDefaultsJsonSerializerContext : JsonSerializerContext;
