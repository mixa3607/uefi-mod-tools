using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.UefiEditorJs.Models;

namespace ArkProjects.UefiModTools.Commands.UefiEditorJs;

[JsonSerializable(typeof(BiosSection))]
[JsonSerializable(typeof(Data))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
internal partial class UefiEditorJsJsonSerializerContext : JsonSerializerContext
{
}
