using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Misc;

public class HexConverter : JsonConverter<int>
{

    public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return Convert.ToInt32(value, 16);
    }

    public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}