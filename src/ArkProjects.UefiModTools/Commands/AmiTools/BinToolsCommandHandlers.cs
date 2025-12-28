using Microsoft.Extensions.Logging;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkProjects.UefiModTools.Commands.AmiTools;

public class AmiToolsCommandHandlers
{
    private readonly string[] _hashSumKeys = new[]
    {
        "megarac",
        "megaracsp",
        "megaracsp2",
        "megaracspx",
        "megarac1",
        "magarac2",
        "megarac3",
        "megarac4",
        "megarac5",
        "megarac6"
    };

    private readonly ILogger<AmiToolsCommandHandlers> _logger;

    public AmiToolsCommandHandlers(ILogger<AmiToolsCommandHandlers> logger)
    {
        _logger = logger;
    }

    private byte[] CalculateSign(byte[] data, int keyId)
    {
        var key = Encoding.ASCII.GetBytes($"\nKEY={_hashSumKeys[keyId]}");
        var allBytes = data.Concat(key).ToArray();

        var sha1 = SHA1.HashData(allBytes).Reverse().ToArray();
        var realSha1Hex = Convert.ToHexString(sha1).ToLowerInvariant();
        var buggedSha1Hex = realSha1Hex.Select((x, i) => i < 2 ? (byte)x : (byte)0x00).ToArray();

        return buggedSha1Hex;
    }

    private (byte[] data, byte[] sign) SplitToDataAndSign(byte[] dump)
    {
        var hash = dump.AsSpan(dump.Length - 40).ToArray();
        var data = dump.AsSpan(0, dump.Length - 40).ToArray();
        return (data, hash);
    }

    private int ReadVersion(BinaryReader reader)
    {
        var verStr = ReadLine(reader);
        var verMatch = Regex.Match(verStr, @"^\$\$\$Version=(?<ver>\d+)\$$");
        if (!verMatch.Success)
        {
            throw new Exception("Can not read version");
        }

        return int.Parse(verMatch.Groups["ver"].Value);
    }

    private int ReadKeyIndex(BinaryReader reader)
    {
        var keyIdsStr = ReadLine(reader);
        var keyIdxMatch = Regex.Match(keyIdsStr, @"^\$\$\$CheckSumKeyIndex=(?<idx>\d+)\$$");
        if (!keyIdxMatch.Success)
        {
            throw new Exception("Can not read key index");
        }

        return int.Parse(keyIdxMatch.Groups["idx"].Value);
    }

    public int PackBak(string inputDirectory, string outputFile)
    {
        var mem = new MemoryStream();
        var writer = new BinaryWriter(mem);

        const int keyIdx = 1;
        writer.Write(Encoding.ASCII.GetBytes($"$$$Version=1$\n"));
        writer.Write(Encoding.ASCII.GetBytes($"$$$CheckSumKeyIndex={keyIdx}$\n"));

        var files = CommandHelpers.ReadString(Path.Combine(inputDirectory, ".files-list"), null, _logger)
            .Split('\n', '\r')
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToArray();
        foreach (var bakFile in files)
        {
            var fsFile = bakFile;
            if (Path.IsPathRooted(fsFile))
                fsFile = Path.GetRelativePath("/", fsFile);
            fsFile = Path.Combine(inputDirectory, fsFile);

            _logger.LogInformation("Appending {file} to bak", bakFile);

            writer.Write((byte)'\n');

            var fileBytes = File.ReadAllBytes(fsFile);
            writer.Write(Encoding.ASCII.GetBytes($"[$$${bakFile}]\n"));
            writer.Write(Encoding.ASCII.GetBytes($"$$$DataLength={fileBytes.Length}$\n"));
            writer.Write(fileBytes);
        }

        writer.Flush();
        var bytes = mem.ToArray();
        var sign = CalculateSign(bytes, keyIdx);


        CommandHelpers.WriteResult(bytes.Concat(sign).ToArray(), outputFile, true, _logger);
        return 0;
    }

    public int UnpackBak(string inputFile, string outputDirectory)
    {
        var inputBytes = CommandHelpers.ReadBytes(inputFile, _logger);
        var (data, sign) = SplitToDataAndSign(inputBytes);

        using var mem = new MemoryStream(data);
        using var binReader = new BinaryReader(mem);

        using var outMem = new MemoryStream();
        using var writer = new BinaryWriter(outMem);

        var ver = ReadVersion(binReader);
        if (ver != 1)
        {
            throw new Exception("Not supported version");
        }

        var keyIdx = ReadKeyIndex(binReader);
        var calculatedSign = CalculateSign(data, keyIdx);
        if (!calculatedSign.SequenceEqual(sign))
        {
            _logger.LogWarning("Calculated sign and in file not same!");
        }

        var files = new List<string>();
        while (mem.Position < mem.Length - 1)
        {
            if (binReader.ReadByte() != '\n')
            {
                throw new Exception("");
            }

            var metaStr = ReadLine(binReader);
            var metaMatch = Regex.Match(metaStr, @"^\[\$\$\$(?<path>.+)\]$");
            if (!metaMatch.Success)
            {
                throw new Exception("");
            }

            var fileName = metaMatch.Groups["path"].Value;
            files.Add(fileName);
            if (Path.IsPathRooted(fileName))
                fileName = Path.GetRelativePath("/", fileName);
            fileName = Path.Combine(outputDirectory, fileName);

            var dataLenStr = ReadLine(binReader);
            var dataLenMatch = Regex.Match(dataLenStr, @"^\$\$\$DataLength=(?<len>\d+)\$$");
            if (!dataLenMatch.Success)
            {
                throw new Exception("");
            }

            var dataLen = int.Parse(dataLenMatch.Groups["len"].Value);
            var fileBytes = binReader.ReadBytes(dataLen);

            CommandHelpers.WriteResult(fileBytes, fileName, true, _logger);
        }

        CommandHelpers.WriteResult(string.Join("\n", files), Path.Combine(outputDirectory, ".files-list"), true, _logger);

        return 0;
    }

    private string ReadLine(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (true)
        {
            var b = reader.ReadByte();
            if (b == (byte)'\n')
            {
                return Encoding.UTF8.GetString(bytes.ToArray());
            }
            else
            {
                bytes.Add(b);
            }
        }
    }
}
