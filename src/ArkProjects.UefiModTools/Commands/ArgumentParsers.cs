using System.CommandLine.Parsing;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Commands;

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
public class CombinedJsonTypeInfoResolver : IJsonTypeInfoResolver
{
    private readonly IReadOnlyList<IJsonTypeInfoResolver> _jsonTypeInfoResolvers;

    public CombinedJsonTypeInfoResolver(IReadOnlyList<IJsonTypeInfoResolver> jsonTypeInfoResolvers)
    {
        _jsonTypeInfoResolvers = jsonTypeInfoResolvers;
    }

    public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
    {
        return _jsonTypeInfoResolvers.Select(x => x.GetTypeInfo(type, options)).FirstOrDefault(x => x != null);
    }
}

public class JsonSerializationService
{
    private readonly JsonSerializerOptions _options;

    public JsonSerializationService(IEnumerable<IJsonTypeInfoResolver> typeInfoResolvers)
    {
        _options = new JsonSerializerOptions()
        {
            TypeInfoResolver = new CombinedJsonTypeInfoResolver(typeInfoResolvers.ToList()),
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter()
            },
        };
    }

    public T Deserialize<T>(string jsonString)
    {
        return JsonSerializer.Deserialize<T>(jsonString, _options);
    }

    public string Serialize(object data)
    {
        return JsonSerializer.Serialize(data, _options);
    }
}

public class ArgumentParsers
{
    public static T NumberParser<T>(ArgumentResult result) where T : struct
    {
        if (result.Tokens.Count == 0)
        {
            return default;
        }
        else if (result.Tokens.Count > 1)
        {
            result.AddError($"Argument --{result.Argument.Name} expects one argument but got {result.Tokens.Count}");
            return default;
        }

        var numBase = 10;
        var numStr = result.Tokens[0].Value.ToLowerInvariant();
        if (numStr.StartsWith("0x", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 16;
        }
        else if (numStr.StartsWith("0b", StringComparison.InvariantCultureIgnoreCase))
        {
            numBase = 2;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

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

        result.AddError($"Argument --{result.Argument.Name} can not be parsed. " +
                        "One of formats expected: 0xDEADBEEF, 3735928559");
        return default;
    }
}
