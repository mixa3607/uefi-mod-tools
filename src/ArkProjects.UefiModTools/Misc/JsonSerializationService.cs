using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Misc;

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