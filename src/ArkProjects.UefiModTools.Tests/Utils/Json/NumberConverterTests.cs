using System.Text.Json;
using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Utils.Json;

public class NumberConverterTests
{
    [Fact]
    public void WritesDecimalNumberWithDefaultConverter()
    {
        var json = JsonSerializer.Serialize(42, CreateOptions(new NumberConverter<int>()));

        Assert.Equal("42", json);
    }

    [Fact]
    public void WritesHexNumber()
    {
        var json = JsonSerializer.Serialize(42, CreateOptions(new NumberConverterAsHex<int>()));

        Assert.Equal("\"0x0000002A\"", json);
    }

    [Fact]
    public void WritesBinaryNumber()
    {
        var json = JsonSerializer.Serialize(42, CreateOptions(new NumberConverterAsBin<int>()));

        Assert.Equal("\"0b00101010\"", json);
    }

    [Theory]
    [InlineData("42")]
    [InlineData("\"0x2A\"")]
    [InlineData("\"0b101010\"")]
    public void ReadsDecimalHexAndBinaryNumbers(string json)
    {
        var value = JsonSerializer.Deserialize<int>(json, CreateOptions(new NumberConverterAsHex<int>()));

        Assert.Equal(42, value);
    }

    [Fact]
    public void RejectsUnsupportedOutputBase()
    {
        var error = Assert.Throws<JsonException>(() =>
            JsonSerializer.Serialize(42, CreateOptions(new NumberConverter<int>(8))));

        Assert.Equal("Unsupported numeric output base 8", error.Message);
    }

    private static JsonSerializerOptions CreateOptions(JsonConverter<int> converter)
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(converter);
        return options;
    }
}
