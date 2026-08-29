using System.Security.Cryptography;
using System.Text;
using ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.AmiTools.BmcBackup;

public class AmiConfigBackupParserTests
{
    [Fact]
    public void CreateBackupRejectsInvalidChecksumKeyIndex()
    {
        var parser = CreateParser();
        var info = new BackupInfoModel { Version = 1, CheckSumKeyIndex = 10 };

        var error = Assert.Throws<Exception>(() => parser.CreateBackup(info, new Dictionary<string, byte[]>()));

        Assert.Contains("Unsupported checksum key index", error.Message);
    }

    [Fact]
    public void ParseBackupRejectsInvalidSignatureUnlessForced()
    {
        var parser = CreateParser();
        var backup = parser.CreateBackup(new BackupInfoModel { Version = 1, CheckSumKeyIndex = 0 },
            new Dictionary<string, byte[]> { ["config"] = [0x01] });
        backup[^1] ^= 1;

        Assert.Throws<InvalidDataException>(() => parser.ParseBackup(backup));

        var (_, files) = parser.ParseBackup(backup, force: true);
        Assert.Equal([0x01], files["config"]);
    }

    [Fact]
    public void ParseBackupRejectsDataLengthLargerThanRemainingPayload()
    {
        var payload = "$$$Version=1$\n$$$CheckSumKeyIndex=0$\n\n[$$$config]\n$$$DataLength=2147483647$\n"u8.ToArray();
        var backup = Sign(payload);

        var error = Assert.Throws<InvalidDataException>(() => CreateParser().ParseBackup(backup));

        Assert.Equal("Backup file config length exceeds the remaining payload", error.Message);
    }

    private static AmiConfigBackupParser CreateParser() => new(NullLogger<AmiConfigBackupParser>.Instance);

    private static byte[] Sign(byte[] payload)
    {
        var key = Encoding.ASCII.GetBytes("\nKEY=megarac");
        var signature = Encoding.ASCII.GetBytes(Convert.ToHexString(SHA1.HashData(payload.Concat(key).ToArray())).ToLowerInvariant());
        return payload.Concat(signature).ToArray();
    }
}
