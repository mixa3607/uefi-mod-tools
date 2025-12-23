using ArkProjects.UefiModTools.Commands.UefiEditorJs;
using ArkProjects.UefiModTools.Smbios.Structures;
using System.Text.Json;
using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Smbios;

namespace ArkProjects.UefiModTools;

[JsonSerializable(typeof(ISmbiosStructure))]
[JsonSerializable(typeof(SmbiosDump))]
[JsonSerializable(typeof(SmbiosRawStructure))]
[JsonSerializable(typeof(IEnumerable<byte>))]
[JsonSerializable(typeof(List<byte>))]
//
[JsonSerializable(typeof(BiosSection))]
[JsonSerializable(typeof(Data))]
//[JsonSerializable(typeof(Menu))]
//[JsonSerializable(typeof(Form))]
//[JsonSerializable(typeof(FormChild))]
internal partial class JsonSourceGenerationContext : JsonSerializerContext
{
}
public class ByteArrayConverter : JsonConverter<byte[]>
{
    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, value.AsEnumerable(), options);

    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.String => reader.GetBytesFromBase64(),
            JsonTokenType.StartArray => JsonSerializer.Deserialize<List<byte>>(ref reader, options)!.ToArray(),
            JsonTokenType.Null => null,
            _ => throw new JsonException(),
        };
}