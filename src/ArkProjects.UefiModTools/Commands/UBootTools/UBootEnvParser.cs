using System.IO.Hashing;
using System.Text;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class UBootEnvParser
{
    private readonly ILogger<UBootEnvParser> _logger;

    public UBootEnvParser(ILogger<UBootEnvParser> logger)
    {
        _logger = logger;
    }

    public UBootEnv Parse(byte[] data)
    {
        using var dataStream = new MemoryStream(data);
        using var dataReader = new BinaryReader(dataStream);

        var padTailLen = data.Reverse().TakeWhile(x => x == 0xFF).Count();
        var savedHash = dataReader.ReadUInt32();

        var payloadSpan = data.AsSpan(sizeof(uint), (int)dataStream.Length - sizeof(uint) - padTailLen);
        var calculatedHash = CalculateEnvVarsHash(payloadSpan);
        if (savedHash != calculatedHash)
            _logger.LogWarning("Detected CRC32 hash mismatch!");
        else
            _logger.LogInformation("CRC32 hash matched");

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
            Hash = calculatedHash,
            Size = data.Length,
            Variables = envVars,
            PaddingSize = padTailLen,
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
