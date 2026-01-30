using System.ComponentModel;
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

[AttributeUsage(AttributeTargets.Property)]
public class NumberConverterAttribute : JsonConverterAttribute
{
    private readonly int _writeBase;

    public NumberConverterAttribute() : this(10)
    {
    }

    public NumberConverterAttribute(int writeBase)
    {
        _writeBase = writeBase;
    }

    public override JsonConverter CreateConverter(Type typeToConvert)
    {
        return new NumberConverter(_writeBase);
    }
}

public class NumberConverter : JsonConverter<object>
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
        var targetType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
        return targetType == typeof(int) || targetType == typeof(uint) ||
               targetType == typeof(byte) || targetType == typeof(sbyte) ||
               targetType == typeof(short) || targetType == typeof(ushort) ||
               targetType == typeof(long) || targetType == typeof(ulong) ||
               false;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var targetType = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (targetType == typeof(byte))
                return reader.GetByte();
            else if (targetType == typeof(sbyte))
                return reader.GetSByte();

            if (targetType == typeof(short))
                return reader.GetInt16();
            else if (targetType == typeof(ushort))
                return reader.GetUInt16();

            if (targetType == typeof(int))
                return reader.GetInt32();
            else if (targetType == typeof(uint))
                return reader.GetUInt32();

            if (targetType == typeof(long))
                return reader.GetInt64();
            else if (targetType == typeof(ulong))
                return reader.GetUInt64();

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
            return Convert.ToByte(numStr, numBase);
        else if (targetType == typeof(sbyte))
            return Convert.ToSByte(numStr, numBase);

        if (targetType == typeof(short))
            return Convert.ToInt16(numStr, numBase);
        else if (targetType == typeof(ushort))
            return Convert.ToUInt16(numStr, numBase);

        if (targetType == typeof(int))
            return Convert.ToInt32(numStr, numBase);
        else if (targetType == typeof(uint))
            return Convert.ToUInt32(numStr, numBase);

        if (targetType == typeof(long))
            return Convert.ToInt64(numStr, numBase);
        else if (targetType == typeof(ulong))
            return Convert.ToUInt64(numStr, numBase);

        throw new Exception();
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        var targetType = Nullable.GetUnderlyingType(value.GetType()) ?? value.GetType();
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
                throw new Exception();
        }

        var (prefix, format) = WriteBase switch
        {
            2 => ("0b", "b8"),
            16 => ("0x", "X8"),
            _ => throw new Exception(),
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

        throw new Exception();
    }
}
