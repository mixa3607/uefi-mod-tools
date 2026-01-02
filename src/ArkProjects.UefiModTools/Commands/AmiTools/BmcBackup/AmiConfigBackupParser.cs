using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcBackup;

public class AmiConfigBackupParser
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

    private readonly ILogger<AmiConfigBackupParser> _logger;

    public AmiConfigBackupParser(ILogger<AmiConfigBackupParser> logger)
    {
        _logger = logger;
    }

    public byte[] CreateBackup(IReadOnlyDictionary<string, byte[]> files, bool isBuggedSha1)
    {
        const int keyIdx = 1;
        const int version = 1;

        var memStream = new MemoryStream();
        var memWriter = new BinaryWriter(memStream);

        memWriter.Write(GetBytes($"$$$Version={version}$\n"));
        memWriter.Write(GetBytes($"$$$CheckSumKeyIndex={keyIdx}$\n"));

        foreach (var (fileName, fileData) in files)
        {
            _logger.LogInformation("Appending {file} to bak", fileName);
            memWriter.Write((byte)'\n');

            memWriter.Write(Encoding.ASCII.GetBytes($"[$$${fileName}]\n"));
            memWriter.Write(Encoding.ASCII.GetBytes($"$$$DataLength={fileData.Length}$\n"));
            memWriter.Write(fileData);
        }

        memWriter.Flush();
        var bytes = memStream.ToArray();
        var sign = CalculateSign(bytes, keyIdx, isBuggedSha1);

        return bytes.Concat(sign).ToArray();
    }

    public IReadOnlyDictionary<string, byte[]> ParseBackup(byte[] backupBytes)
    {
        var (payload, sign) = SplitToDataAndSign(backupBytes);
        using var payloadStream = new MemoryStream(payload);
        using var payloadReader = new BinaryReader(payloadStream);

        var ver = ReadVersion(payloadReader);
        if (ver != 1)
        {
            throw new Exception("Not supported version");
        }

        var keyIdx = ReadKeyIndex(payloadReader);
        var isBuggedSha1 = sign.Count(x => x != 0x00) == 2;
        if (isBuggedSha1)
            _logger.LogWarning("Bugged hash calculation detected. Use --bugged-sha with pack-bak operation");

        var calculatedSign = CalculateSign(payload, keyIdx, isBuggedSha1);
        if (!calculatedSign.SequenceEqual(sign))
            _logger.LogWarning("Calculated sign and in file not same!");

        var files = new Dictionary<string, byte[]>();
        while (payloadStream.Position < payloadStream.Length - 1)
        {
            if (payloadReader.ReadByte() != '\n')
                throw new Exception("Error in backup file format");

            var metaStr = ReadLine(payloadReader);
            var metaMatch = Regex.Match(metaStr, @"^\[\$\$\$(?<path>.+)\]$");
            if (!metaMatch.Success)
                throw new Exception("Can not parse metadata line");
            var fileName = metaMatch.Groups["path"].Value;

            var dataLenStr = ReadLine(payloadReader);
            var dataLenMatch = Regex.Match(dataLenStr, @"^\$\$\$DataLength=(?<len>\d+)\$$");
            if (!dataLenMatch.Success)
                throw new Exception("Can not parse data len line");
            var dataLen = int.Parse(dataLenMatch.Groups["len"].Value);

            files[fileName] = payloadReader.ReadBytes(dataLen);
        }

        return files;
    }

    /// <summary>
    /// Generate sign
    /// </summary>
    /// <param name="payload">Payload bytes</param>
    /// <param name="keyId">Key index</param>
    /// <param name="isBuggedSha1">In some implementations SHA1 str contains only first byte</param>
    /// <returns></returns>
    private byte[] CalculateSign(byte[] payload, int keyId, bool isBuggedSha1)
    {
        var key = GetBytes($"\nKEY={_hashSumKeys[keyId]}");
        var allBytes = payload.Concat(key).ToArray();

        var sha1 = SHA1.HashData(allBytes).Reverse().ToArray();
        var sign = Convert.ToHexString(sha1)
            .ToLowerInvariant()
            .Select((x, i) => i < 2 && isBuggedSha1 ? (byte)x : (byte)0x00)
            .ToArray();
        return sign;
    }

    private byte[] GetBytes(string data) => Encoding.ASCII.GetBytes(data);

    private (byte[] data, byte[] sign) SplitToDataAndSign(byte[] dump)
    {
        var hash = dump.Take(40).ToArray();
        var data = dump.Skip(40).ToArray();
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
