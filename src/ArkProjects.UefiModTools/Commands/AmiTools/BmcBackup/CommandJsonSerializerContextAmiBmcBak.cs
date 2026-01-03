using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;

[JsonSerializable(typeof(List<BackupInfoModel>))]
internal partial class CommandJsonSerializerContextAmiBmcBak : JsonSerializerContext
{
}
