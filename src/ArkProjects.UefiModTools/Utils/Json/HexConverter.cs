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

public class HexConverter2<T> : JsonConverter<T> where T : struct
{
    public override bool CanConvert(Type typeToConvert)
    {
        return Type == typeof(int) || Type == typeof(uint) ||
               Type == typeof(byte) || Type == typeof(sbyte);
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (targetType == typeof(byte))
                return (T)(object)reader.GetByte();
            else if (targetType == typeof(sbyte))
                return (T)(object)reader.GetSByte();

            if (targetType == typeof(short))
                return (T)(object)reader.GetInt16();
            else if (targetType == typeof(ushort))
                return (T)(object)reader.GetUInt16();

            if (targetType == typeof(int))
                return (T)(object)reader.GetInt32();
            else if (targetType == typeof(uint))
                return (T)(object)reader.GetUInt32();

            if (targetType == typeof(long))
                return (T)(object)reader.GetInt64();
            else if (targetType == typeof(ulong))
                return (T)(object)reader.GetUInt64();

            throw new Exception();
        }

        var numStr = reader.GetString()!;
        var numBase = 10;
        if (numStr.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 16;
        }
        else if (numStr.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 2;
        }


        if (targetType == typeof(byte))
            return (T)(object)Convert.ToByte(numStr, numBase);
        else if (targetType == typeof(sbyte))
            return (T)(object)Convert.ToSByte(numStr, numBase);

        if (targetType == typeof(short))
            return (T)(object)Convert.ToInt16(numStr, numBase);
        else if (targetType == typeof(ushort))
            return (T)(object)Convert.ToUInt16(numStr, numBase);

        if (targetType == typeof(int))
            return (T)(object)Convert.ToInt32(numStr, numBase);
        else if (targetType == typeof(uint))
            return (T)(object)Convert.ToUInt32(numStr, numBase);

        if (targetType == typeof(long))
            return (T)(object)Convert.ToInt64(numStr, numBase);
        else if (targetType == typeof(ulong))
            return (T)(object)Convert.ToUInt64(numStr, numBase);

        throw new Exception();
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        string? hex = null;

        if (value is byte b)
            hex = "0x" + b.ToString("X8");
        else if (value is byte sb)
            hex = "0x" + sb.ToString("X8");

        if (value is int i)
            hex = "0x" + i.ToString("X8");
        else if (value is uint ui)
            hex = "0x" + ui.ToString("X8");

        if (hex != null)
        {
            writer.WriteStringValue(hex);
        }

        throw new Exception();
    }
}
