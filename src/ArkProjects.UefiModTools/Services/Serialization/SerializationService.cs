using System.Buffers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace ArkProjects.UefiModTools.Services.Serialization;

public class SerializationService : ISerializationService
{
    private readonly IJsonSerializationService _jsonSerializationService;
    private readonly ILogger<SerializationService> _logger;

    public SerializationService(IJsonSerializationService jsonSerializationService, ILogger<SerializationService> logger)
    {
        _jsonSerializationService = jsonSerializationService;
        _logger = logger;
    }

    public T Deserialize<T>(string input, SerializationFormat format = SerializationFormat.Auto)
    {
        if (format is SerializationFormat.Auto or SerializationFormat.Yaml)
        {
            _logger.LogDebug("Converting YAML input to JSON");
            input = ConvertToJson(input);
        }

        return _jsonSerializationService.Deserialize<T>(input);
    }

    public string Serialize(object data, SerializationFormat format = SerializationFormat.Auto)
    {
        var output = _jsonSerializationService.Serialize(data);

        if (format is SerializationFormat.Yaml)
        {
            output = ConvertToYaml(output);
        }

        return output;
    }

    private string ConvertToJson(string input)
    {
        var yaml = new YamlStream();
        yaml.Load(new StringReader(input));
        if (yaml.Documents.Count != 1)
            throw new ArgumentException("YAML input must contain exactly one document", nameof(input));

        var output = new ArrayBufferWriter<byte>();
        using var writer = new Utf8JsonWriter(output);
        WriteJson(writer, yaml.Documents[0].RootNode);
        writer.Flush();
        return Encoding.UTF8.GetString(output.WrittenSpan);
    }

    private string ConvertToYaml(string input)
    {
        using var json = JsonDocument.Parse(input);
        var yaml = new YamlStream(new YamlDocument(CreateYamlNode(json.RootElement)));
        using var output = new StringWriter();
        yaml.Save(output, assignAnchors: false);
        return output.ToString();
    }

    private static void WriteJson(Utf8JsonWriter writer, YamlNode node)
    {
        switch (node)
        {
            case YamlMappingNode mapping:
                writer.WriteStartObject();
                foreach (var (key, value) in mapping.Children)
                {
                    if (key is not YamlScalarNode { Value: { } keyValue })
                        throw new ArgumentException("YAML mapping keys must be strings");

                    writer.WritePropertyName(keyValue);
                    WriteJson(writer, value);
                }
                writer.WriteEndObject();
                return;

            case YamlSequenceNode sequence:
                writer.WriteStartArray();
                foreach (var child in sequence.Children)
                    WriteJson(writer, child);
                writer.WriteEndArray();
                return;

            case YamlScalarNode scalar:
                WriteJsonScalar(writer, scalar);
                return;

            default:
                throw new ArgumentException($"YAML node type {node.GetType().Name} cannot be represented in JSON");
        }
    }

    private static void WriteJsonScalar(Utf8JsonWriter writer, YamlScalarNode scalar)
    {
        var value = scalar.Value;
        if (value is null || scalar.Style == ScalarStyle.Plain && value is "null" or "~")
        {
            writer.WriteNullValue();
            return;
        }
        if (scalar.Style == ScalarStyle.Plain && bool.TryParse(value, out var boolean))
        {
            writer.WriteBooleanValue(boolean);
            return;
        }
        if (scalar.Style == ScalarStyle.Plain && IsJsonNumber(value))
        {
            writer.WriteRawValue(value, skipInputValidation: true);
            return;
        }

        writer.WriteStringValue(value);
    }

    private static bool IsJsonNumber(string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);
            return document.RootElement.ValueKind == JsonValueKind.Number;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static YamlNode CreateYamlNode(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.Object => CreateYamlMapping(element),
            JsonValueKind.Array => CreateYamlSequence(element),
            JsonValueKind.String => new YamlScalarNode(element.GetString()) { Style = ScalarStyle.DoubleQuoted },
            JsonValueKind.Number => new YamlScalarNode(element.GetRawText()) { Style = ScalarStyle.Plain },
            JsonValueKind.True => new YamlScalarNode("true") { Style = ScalarStyle.Plain },
            JsonValueKind.False => new YamlScalarNode("false") { Style = ScalarStyle.Plain },
            JsonValueKind.Null => new YamlScalarNode("null") { Style = ScalarStyle.Plain },
            _ => throw new ArgumentException($"JSON value kind {element.ValueKind} cannot be represented in YAML"),
        };
    }

    private static YamlMappingNode CreateYamlMapping(JsonElement element)
    {
        var mapping = new YamlMappingNode();
        foreach (var property in element.EnumerateObject())
        {
            var key = new YamlScalarNode(property.Name) { Style = ScalarStyle.DoubleQuoted };
            mapping.Children.Add(key, CreateYamlNode(property.Value));
        }

        return mapping;
    }

    private static YamlSequenceNode CreateYamlSequence(JsonElement element)
    {
        var sequence = new YamlSequenceNode();
        foreach (var child in element.EnumerateArray())
            sequence.Children.Add(CreateYamlNode(child));

        return sequence;
    }
}
