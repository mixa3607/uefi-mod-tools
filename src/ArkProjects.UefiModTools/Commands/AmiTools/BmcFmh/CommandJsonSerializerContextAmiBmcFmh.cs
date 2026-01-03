using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

[JsonSerializable(typeof(List<IFmhSectionModel>))]
internal partial class CommandJsonSerializerContextAmiBmcFmh : JsonSerializerContext
{
}
