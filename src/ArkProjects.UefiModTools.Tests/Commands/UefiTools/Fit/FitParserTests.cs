using ArkProjects.UefiModTools.Commands.UefiTools.Fit;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.Fit;

public class FitParserTests
{
    [Fact]
    public void ReadRejectsTruncatedHeader()
    {
        var error = Assert.Throws<Exception>(() => new FitParser().Read("_FIT_   "u8.ToArray()));

        Assert.Equal("FIT header is truncated", error.Message);
    }

    [Fact]
    public void WriteRejectsHeaderWithMismatchedEntryCount()
    {
        var table = new FitTable
        {
            Entries = [new FitEntry { Type = FitEntryType.FitHeaderEntry, Size = 2 }],
        };

        var error = Assert.Throws<ArgumentException>(() => new FitParser().Write(table));

        Assert.Contains("entry count does not match", error.Message);
    }

    [Fact]
    public void ReadWriteRoundTripsKnownFitTable()
    {
        var source = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "test-files", "FIT_table_base.bin"));
        var parser = new FitParser();

        var table = parser.Read(source);

        Assert.NotEmpty(table.Entries);
        Assert.Equal(source, parser.Write(table));
    }
}
