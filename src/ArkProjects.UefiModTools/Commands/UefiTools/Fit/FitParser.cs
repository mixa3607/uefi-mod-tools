using System.Buffers.Binary;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class FitParser
{
    private const int FitEntrySize = 16;

    public byte[] Write(FitTable fitTable)
    {
        var headGarbageStart = 0;
        var headGarbageEnd = fitTable.HeadGarbage.Length;
        var fitHeadStart = headGarbageEnd;
        var fitHeadEnd = fitHeadStart + fitTable.Entries.Count * FitEntrySize;
        var tailGarbageStart = fitHeadEnd;
        var tailGarbageEnd = tailGarbageStart + fitTable.TailGarbage.Length;
        var fitBytes = new byte[tailGarbageEnd];

        fitTable.HeadGarbage.CopyTo(fitBytes, headGarbageStart);
        fitTable.TailGarbage.CopyTo(fitBytes, tailGarbageStart);
        for (int i = 0; i < fitTable.Entries.Count; i++)
        {
            WriteEntry(fitTable.Entries[i], fitBytes.AsSpan(fitHeadStart + i * FitEntrySize, FitEntrySize));
        }

        return fitBytes;
    }

    public FitTable Read(byte[] fitBytes)
    {
        var fitHeadMarker = "_FIT_   "u8;

        var begin = -1;
        for (int i = 0; i <= fitBytes.Length - fitHeadMarker.Length; i++)
        {
            var isStart = fitBytes.AsSpan(i, fitHeadMarker.Length).SequenceEqual(fitHeadMarker);
            if (isStart)
            {
                begin = i;
                break;
            }
        }

        if (begin < 0)
        {
            throw new Exception("Can not locate FIT begin");
        }

        var headGarbage = fitBytes.AsSpan(0, begin).ToArray();
        if (fitBytes.Length - begin < FitEntrySize)
            throw new Exception("FIT header is truncated");

        var headEntry = ReadEntry(fitBytes.AsSpan(begin, FitEntrySize));
        if (headEntry.Type != FitEntryType.FitHeaderEntry)
        {
            throw new Exception($"Expected first entry is \"FIT Header Entry\" but read \"{headEntry.Type}\"");
        }

        if (headEntry.Size == 0 || headEntry.Size > (fitBytes.Length - begin) / FitEntrySize)
            throw new Exception("FIT header specifies an invalid entry count");

        var fitLength = checked((int)headEntry.Size * FitEntrySize);
        var tailGarbage = fitBytes.AsSpan(begin + fitLength).ToArray();

        var fitTable = new FitTable
        {
            HeadGarbage = headGarbage,
            Entries = [headEntry],
            TailGarbage = tailGarbage,
        };

        for (int i = 1; i < headEntry.Size; i++)
        {
            var entry = ReadEntry(fitBytes.AsSpan(begin + i * FitEntrySize, FitEntrySize));
            fitTable.Entries.Add(entry);
        }

        return fitTable;
    }

    private FitEntry ReadEntry(Span<byte> entryBytes)
    {
        var pos = 0;

        var address = BinaryPrimitives.ReadUInt64LittleEndian(entryBytes.Slice(pos, 8));
        pos += 8;

        var size = (BinaryPrimitives.ReadUInt32LittleEndian(entryBytes.Slice(pos, 4)) << 8) >> 8;
        pos += 3;

        var reserved = entryBytes[pos];
        pos += 1;

        var version = BinaryPrimitives.ReadUInt16LittleEndian(entryBytes.Slice(pos, 2));
        pos += 2;

        var checksumValid = entryBytes[pos] >= 0b10000000;
        var type = (FitEntryType)(entryBytes[pos] & 0b01111111);
        pos += 1;

        var checksum = entryBytes[pos];
        pos += 1;

        return new FitEntry()
        {
            Address = address,
            Size = size,
            Reserved = reserved,
            Version = version,
            ChecksumValidate = checksumValid,
            Type = type,
            Checksum = checksum
        };
    }

    private void WriteEntry(FitEntry entry, Span<byte> entryBytes)
    {
        var pos = 0;

        BinaryPrimitives.WriteUInt64LittleEndian(entryBytes.Slice(pos, 8), entry.Address);
        pos += 8;

        BinaryPrimitives.WriteUInt32LittleEndian(entryBytes.Slice(pos, 4), entry.Size);
        pos += 3;

        entryBytes[pos] = entry.Reserved;
        pos += 1;

        BinaryPrimitives.WriteUInt16LittleEndian(entryBytes.Slice(pos, 2), entry.Version);
        pos += 2;

        entryBytes[pos] = entry.ChecksumValidate ? (byte)0b10000000 : (byte)0b00000000;
        entryBytes[pos] += (byte)((byte)entry.Type & 0b01111111);
        pos += 1;

        entryBytes[pos] = entry.Checksum;
        pos += 1;
    }
}
