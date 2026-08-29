using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Utils;

public class NumberConverterAsHex<T>() : NumberConverter<T>(16) where T : struct;
public class NumberConverterAsBin<T>() : NumberConverter<T>(2) where T : struct;

public class NumberConverter<T> : JsonConverter<T> where T : struct
{
    public int WriteBase;

    public NumberConverter() : this(10)
    {
    }

    public NumberConverter(int writeBase)
    {
        WriteBase = writeBase;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return targetType == typeof(int) || targetType == typeof(uint) ||
               targetType == typeof(byte) || targetType == typeof(sbyte) ||
               targetType == typeof(short) || targetType == typeof(ushort) ||
               targetType == typeof(long) || targetType == typeof(ulong) ||
               false;
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

            throw new JsonException($"Unsupported numeric type {targetType}");
        }

        var numStr = reader.GetString()!;
        var numBase = 10;
        if (numStr.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 16;
            numStr = numStr[2..];
        }
        else if (numStr.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 2;
            numStr = numStr[2..];
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

        throw new JsonException($"Unsupported numeric type {targetType}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (WriteBase == 10)
        {
            if (targetType == typeof(byte))
                writer.WriteNumberValue((byte)(object)value);
            else if (targetType == typeof(sbyte))
                writer.WriteNumberValue((byte)(object)value);

            else if (targetType == typeof(short))
                writer.WriteNumberValue((short)(object)value);
            else if (targetType == typeof(ushort))
                writer.WriteNumberValue((ushort)(object)value);

            else if (targetType == typeof(int))
                writer.WriteNumberValue((int)(object)value);
            else if (targetType == typeof(uint))
                writer.WriteNumberValue((uint)(object)value);

            else if (targetType == typeof(long))
                writer.WriteNumberValue((long)(object)value);
            else if (targetType == typeof(ulong))
                writer.WriteNumberValue((ulong)(object)value);

            else
                throw new JsonException($"Unsupported numeric type {targetType}");

            return;
        }

        var (prefix, format) = WriteBase switch
        {
            2 => ("0b", "b8"),
            16 => ("0x", "X8"),
            _ => throw new JsonException($"Unsupported numeric output base {WriteBase}"),
        };

        string? str = null;

        if (value is byte b)
            str = prefix + b.ToString(format);
        else if (value is sbyte sb)
            str = prefix + sb.ToString(format);

        if (value is short s)
            str = prefix + s.ToString(format);
        else if (value is ushort us)
            str = prefix + us.ToString(format);

        if (value is int i)
            str = prefix + i.ToString(format);
        else if (value is uint ui)
            str = prefix + ui.ToString(format);

        if (value is long l)
            str = prefix + l.ToString(format);
        else if (value is ulong ul)
            str = prefix + ul.ToString(format);

        if (str != null)
        {
            writer.WriteStringValue(str);
            return;
        }

        throw new JsonException($"Unsupported numeric type {targetType}");
    }
}
