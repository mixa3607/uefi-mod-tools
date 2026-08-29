using ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;
using ArkProjects.UefiModTools.Commands.UefiTools;
using ArkProjects.UefiModTools.Smbios;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests;

public class BinaryFormatValidationTests
{
    [Fact]
    public void FitParserRejectsTruncatedHeader()
    {
        var data = "_FIT_   "u8.ToArray();
        var parser = new FitParser();

        var error = Assert.Throws<Exception>(() => parser.Read(data));

        Assert.Equal("FIT header is truncated", error.Message);
    }

    [Fact]
    public void SmbiosReaderRejectsInvalidStructureLength()
    {
        var reader = new SmbiosReader();
        using var stream = new MemoryStream([0x00, 0x03, 0x00, 0x00]);

        var error = Assert.Throws<Exception>(() => reader.Read(stream));

        Assert.Contains("invalid length", error.Message);
    }

    [Fact]
    public void SmbiosWriterRejectsOversizedFormattedBody()
    {
        var writer = new SmbiosWriter();
        var dump = new SmbiosDump
        {
            Length = 512,
            Structures = [new SmbiosRawStructure { Body = new byte[252] }]
        };

        Assert.Throws<ArgumentException>(() => writer.Write(dump, new MemoryStream()));
    }

    [Fact]
    public void BackupParserRejectsInvalidChecksumKeyIndex()
    {
        var parser = new AmiConfigBackupParser(NullLogger<AmiConfigBackupParser>.Instance);
        var info = new BackupInfoModel { Version = 1, CheckSumKeyIndex = 10 };

        var error = Assert.Throws<Exception>(() => parser.CreateBackup(info, new Dictionary<string, byte[]>()));

        Assert.Contains("Unsupported checksum key index", error.Message);
    }
}
