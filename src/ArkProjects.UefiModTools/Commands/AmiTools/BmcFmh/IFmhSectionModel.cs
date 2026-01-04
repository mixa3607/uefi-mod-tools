using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;


[JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(Type))]
[JsonDerivedType(typeof(FmhTailSectionModel), typeDiscriminator: FmhTailSectionModel.SectionType)]
[JsonDerivedType(typeof(FmhSectionModel), typeDiscriminator: FmhSectionModel.SectionType)]
public interface IFmhSectionModel
{
    [JsonIgnore]
    string Type { get; }

    [JsonConverter(typeof(HexConverter))]
    int BeginAddress { get; set; }

    [JsonConverter(typeof(HexConverter))]
    int EndAddress { get; set; }

    long Length => EndAddress - BeginAddress;
}
