using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Commands.BinTools.Models;

namespace ArkProjects.UefiModTools.Commands.BinTools;

[JsonSerializable(typeof(PartitionsTable))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
internal partial class BinJsonSerializerContext : JsonSerializerContext
{
}
