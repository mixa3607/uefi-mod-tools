using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Utils;

public class HexConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Convert.ToInt32(value, 16);
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        var hex = "0x" + value.ToString("X8");
        writer.WriteStringValue(hex);
    }
}
