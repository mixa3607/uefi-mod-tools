using System.Text.Json.Serialization.Metadata;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Services.Serialization;

public class SerializationServiceTests
{
    [Theory]
    [InlineData(SerializationFormat.Json)]
    [InlineData(SerializationFormat.Auto)]
    public void DeserializeReadsJson(SerializationFormat format)
    {
        var result = CreateService().Deserialize<TestDocument>("{ \"name\": \"example\", \"value\": 42 }", format);

        Assert.Equal("example", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Theory]
    [InlineData(SerializationFormat.Yaml)]
    [InlineData(SerializationFormat.Auto)]
    public void DeserializeReadsYaml(SerializationFormat format)
    {
        var result = CreateService().Deserialize<TestDocument>("name: example\nvalue: 42\n", format);

        Assert.Equal("example", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Theory]
    [InlineData(SerializationFormat.Json)]
    [InlineData(SerializationFormat.Auto)]
    public void SerializeWritesJson(SerializationFormat format)
    {
        var output = CreateService().Serialize(new TestDocument { Name = "example", Value = 42 }, format);
        var result = CreateService().Deserialize<TestDocument>(output, SerializationFormat.Json);

        Assert.Equal("example", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void SerializeWritesYaml()
    {
        var service = CreateService();
        var output = service.Serialize(new TestDocument { Name = "example", Value = 42 }, SerializationFormat.Yaml);
        var result = service.Deserialize<TestDocument>(output, SerializationFormat.Yaml);

        Assert.Contains("name: \"example\"", output);
        Assert.Equal("example", result.Name);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void YamlRoundTripPreservesStringScalars()
    {
        var service = CreateService();
        var yaml = service.Serialize(new StringScalars { Number = "42", Boolean = "true", Null = "null" }, SerializationFormat.Yaml);
        var result = service.Deserialize<StringScalars>(yaml, SerializationFormat.Yaml);

        Assert.Equal("42", result.Number);
        Assert.Equal("true", result.Boolean);
        Assert.Equal("null", result.Null);
    }

    [Fact]
    public void YamlRoundTripPreservesNestedTree()
    {
        var service = CreateService();
        var source = new NestedDocument
        {
            Name = "root",
            Values = [1, 2, 3],
            Child = new NestedChild
            {
                Enabled = true,
                Items =
                [
                    new NestedItem { Name = "first", Value = 10 },
                    new NestedItem { Name = "second", Value = 20 },
                ],
            },
        };

        var yaml = service.Serialize(source, SerializationFormat.Yaml);
        var result = service.Deserialize<NestedDocument>(yaml, SerializationFormat.Yaml);

        Assert.Equal("root", result.Name);
        Assert.Equal([1, 2, 3], result.Values);
        Assert.True(result.Child.Enabled);
        Assert.Collection(result.Child.Items,
            item => Assert.Equal(("first", 10), (item.Name, item.Value)),
            item => Assert.Equal(("second", 20), (item.Name, item.Value)));
    }

    private static SerializationService CreateService()
    {
        var jsonService = new JsonSerializationService([new DefaultJsonTypeInfoResolver()]);
        return new SerializationService(jsonService, NullLogger<SerializationService>.Instance);
    }

    private class TestDocument
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }

    private class StringScalars
    {
        public string Number { get; set; } = string.Empty;
        public string Boolean { get; set; } = string.Empty;
        public string Null { get; set; } = string.Empty;
    }

    private class NestedDocument
    {
        public string Name { get; set; } = string.Empty;
        public List<int> Values { get; set; } = [];
        public NestedChild Child { get; set; } = new();
    }

    private class NestedChild
    {
        public bool Enabled { get; set; }
        public List<NestedItem> Items { get; set; } = [];
    }

    private class NestedItem
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }
    }
}
