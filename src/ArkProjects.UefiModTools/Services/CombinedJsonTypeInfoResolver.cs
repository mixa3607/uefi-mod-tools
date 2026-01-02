using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace ArkProjects.UefiModTools.Misc;

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
