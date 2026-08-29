using ArkProjects.UefiModTools.Smbios;
using Xunit;

namespace ArkProjects.UefiModTools.Tests;

public class SmbiosValidationTests
{
    [Fact]
    public void ReaderRejectsInvalidStructureLength()
    {
        var reader = new SmbiosReader();
        using var stream = new MemoryStream([0x00, 0x03, 0x00, 0x00]);

        var error = Assert.Throws<Exception>(() => reader.Read(stream));

        Assert.Contains("invalid length", error.Message);
    }

    [Fact]
    public void WriterRejectsOversizedFormattedBody()
    {
        var dump = new SmbiosDump
        {
            Length = 512,
            Structures = [new SmbiosRawStructure { Body = new byte[252] }],
        };

        Assert.Throws<ArgumentException>(() => new SmbiosWriter().Write(dump, new MemoryStream()));
    }
}
