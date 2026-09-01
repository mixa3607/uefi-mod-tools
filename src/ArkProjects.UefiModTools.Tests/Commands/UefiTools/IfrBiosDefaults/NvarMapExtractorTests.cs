using ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrBiosDefaults;

public class NvarMapExtractorTests
{
    [Fact]
    public void ExtractReadsNestedNvarRecords()
    {
        var child = CreateRecord("Child", NvarAttributes.DataOnly, []);
        var source = CreateRecord("Parent", NvarAttributes.RuntimeVariable, child);

        var variables = CreateExtractor().Extract(source);

        Assert.Collection(variables,
            parent =>
            {
                Assert.Equal("Parent", parent.Name);
                Assert.Equal(NvarAttributes.RuntimeVariable, parent.Attributes);
                Assert.Equal(-1, parent.ParentRecordOffset);
                Assert.Equal(0, parent.RecordOffset);
                Assert.Equal(18, parent.DataOffset);
            },
            childVariable =>
            {
                Assert.Equal("Child", childVariable.Name);
                Assert.Equal(NvarAttributes.DataOnly, childVariable.Attributes);
                Assert.Equal(0, childVariable.ParentRecordOffset);
                Assert.Equal(18, childVariable.RecordOffset);
            });
    }

    [Fact]
    public void ExtractStopsAtDataAfterRecords()
    {
        var source = CreateRecord("BootOrder", NvarAttributes.AsciiName, [])
            .Concat(new byte[] { 0xFF, 0x00 })
            .ToArray();

        var variable = Assert.Single(CreateExtractor().Extract(source));

        Assert.Equal("BootOrder", variable.Name);
    }

    [Fact]
    public void ExtractRejectsRecordWithUnterminatedName()
    {
        var source = CreateRecord("Name", NvarAttributes.None, []);
        source[^1] = 0x41;

        var error = Assert.Throws<InvalidDataException>(() => CreateExtractor().Extract(source));

        Assert.Equal("NVAR record name is not null-terminated", error.Message);
    }

    [Fact]
    public void ExtractRejectsChainedRecords()
    {
        var source = CreateRecord("Name", NvarAttributes.None, [], next: 0);

        var error = Assert.Throws<NotSupportedException>(() => CreateExtractor().Extract(source));

        Assert.Equal("Chained NVAR records are not supported. Next=0x000000", error.Message);
    }

    private static NvarMapExtractor CreateExtractor() => new(NullLogger<NvarMapExtractor>.Instance);

    private static byte[] CreateRecord(string name, NvarAttributes attributes, byte[] data, int next = 0xFFFFFF)
    {
        var nameBytes = System.Text.Encoding.ASCII.GetBytes(name);
        var recordSize = checked((ushort)(11 + nameBytes.Length + 1 + data.Length));
        var record = new List<byte>(recordSize);
        record.AddRange("NVAR"u8.ToArray());
        record.AddRange(BitConverter.GetBytes(recordSize));
        record.Add((byte)next);
        record.Add((byte)(next >> 8));
        record.Add((byte)(next >> 16));
        record.Add((byte)attributes);
        record.Add(0);
        record.AddRange(nameBytes);
        record.Add(0);
        record.AddRange(data);
        return record.ToArray();
    }
}
