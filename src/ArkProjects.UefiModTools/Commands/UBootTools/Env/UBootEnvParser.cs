using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.IO.Hashing;
using System.Text;

namespace ArkProjects.UefiModTools.Commands.UBootTools.Env;

public class UBootEnvParser
{
    private readonly ILogger<UBootEnvParser> _logger;

    public UBootEnvParser(ILogger<UBootEnvParser> logger)
    {
        _logger = logger;
    }

    private bool CheckSha(Span<byte> allData, int tailSize)
    {
        var savedHash = BinaryPrimitives.ReadUInt32LittleEndian(allData.Slice(0, 4));

        var payloadSpan = allData.Slice(sizeof(uint), allData.Length - sizeof(uint) - tailSize);
        var calculatedHash = CalculateEnvVarsHash(payloadSpan);

        return savedHash == calculatedHash;
    }

    public UBootEnv? Parse(byte[] data, bool breakOnBadHash, int tailSize = -1)
    {
        if (data.All(x => x == 0xFF))
        {
            return null;
        }

        var hashMatched = false;
        int[] tailSizes = tailSize >= 0 ? [tailSize] : [0, 4];
        foreach (var t in tailSizes)
        {
            tailSize = t;
            hashMatched = CheckSha(data, tailSize);
            if (hashMatched)
                break;
        }

        if (!hashMatched)
        {
            if (breakOnBadHash)
                return null;

            _logger.LogWarning("Detected CRC32 hash mismatch!");
        }
        else
        {
            _logger.LogInformation("CRC32 hash matched");
        }

        using var dataStream = new MemoryStream(data, sizeof(uint), data.Length - sizeof(uint), false);
        using var dataReader = new BinaryReader(dataStream);
        var envVars = new Dictionary<string, string>();
        while (true)
        {
            var line = ReadString(dataReader);
            if (line == "")
            {
                break;
            }

            if (line.Split("=", 2) is not [var name, var value])
            {
                _logger.LogWarning("Can not parse line {line} to kv", line);
                continue;
            }

            envVars[name] = value;
        }

        _logger.LogInformation("Read {count} pairs", envVars.Count);

        return new UBootEnv()
        {
            HashMatched = hashMatched,
            Size = data.Length,
            Variables = envVars,
            PaddingSize = tailSize,
        };
    }

    public byte[] Create(UBootEnv env)
    {
        var data = new byte[env.Size];
        using var dataStream = new MemoryStream(data);
        using var dataWriter = new BinaryWriter(dataStream);

        // skip crc32
        dataWriter.Write(0u);

        // write vars
        _logger.LogInformation("Write {count} pairs", env.Variables.Count);
        foreach (var (name, value) in env.Variables)
        {
            var line = GetBytes($"{name}={value}");
            dataWriter.Write(line);
            dataWriter.Write((byte)0x00);
        }

        // finish vars
        dataWriter.Write((byte)0x00);
        dataWriter.Flush();

        dataStream.Position = 0;
        var payloadSpan = data.AsSpan(sizeof(uint), data.Length - sizeof(uint) - env.PaddingSize);
        var hash = CalculateEnvVarsHash(payloadSpan);
        dataWriter.Write(hash);
        dataWriter.Flush();

        // write padding
        Array.Fill(data, (byte)0xFF, data.Length - env.PaddingSize, env.PaddingSize);

        return data;
    }

    public List<UBootEnvInDump> Scan(byte[] bin, int pagesCount, int blockSize, int blocksWindow)
    {
        _logger.LogInformation("Scan {pages} pages with 0x{size:X8} size and sliding window {window} pages",
            pagesCount, blockSize, blocksWindow);

        var result = new List<UBootEnvInDump>();
        for (int i = 0; i < pagesCount - (blocksWindow - 1); i++)
        {
            var pageRange = new Range(i * blockSize, (i * blockSize) + (blocksWindow * blockSize));
            var page = bin
                .AsSpan(pageRange)
                .ToArray();
            var env = Parse(page, true);
            if (env == null)
                continue;

            _logger.LogInformation("Found potential env section in page 0x{start:X8}-0x{end:X8}",
                pageRange.Start.Value, pageRange.End.Value);

            result.Add(new UBootEnvInDump()
            {
                BeginAddress = pageRange.Start.Value,
                EndAddress = pageRange.End.Value,
                Variables = env.Variables,
                PaddingSize = env.PaddingSize,
            });
        }

        return result;
    }

    private uint CalculateEnvVarsHash(ReadOnlySpan<byte> data) => Crc32.HashToUInt32(data);
    private byte[] GetBytes(string data) => Encoding.ASCII.GetBytes(data);

    private string ReadString(BinaryReader reader)
    {
        var bytes = new List<byte>();
        while (true)
        {
            var b = reader.ReadByte();
            if (b == 0x00)
                return Encoding.ASCII.GetString(bytes.ToArray());

            bytes.Add(b);
        }
    }
}
