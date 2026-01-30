using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.Logging;
using System.Buffers.Binary;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly ICommandFileManager _fileManager;

    private readonly byte[] _emptyRecordBytes = new byte[]
    {
        0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x7F, 0x00,
    };

    public CommandHandlers(ILogger<CommandHandlers> logger,
        IJsonSerializationService jsonSerializer, ICommandFileManager fileManager)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
        _fileManager = fileManager;
    }

    private byte[] CreateFitForMCodes(int mcodesCount)
    {
        return Enumerable.Repeat(_emptyRecordBytes, mcodesCount).SelectMany(x => x).ToArray();
    }

    private void InjectMCode2Fit(byte[] fitBytes, uint mcodeFwStart)
    {
        for (int i = 0; i < fitBytes.Length / 16; i++)
        {
            var emptyPlaceSpan = _emptyRecordBytes.AsSpan();
            var fitSpan = fitBytes.AsSpan(i * 16, 16);

            if (emptyPlaceSpan.SequenceEqual(fitSpan))
            {
                var fitBytesSect = new byte[]
                {
                    (byte)(mcodeFwStart >> 0), (byte)(mcodeFwStart >> 8), (byte)(mcodeFwStart >> 16),
                    (byte)(mcodeFwStart >> 24),
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x00, 0x00,
                    0x00, 0x01, 0x01, 0x00,
                };
                _logger.LogInformation("FIT: {bytes}", BitConverter.ToString(fitBytesSect).Replace("-", " "));
                fitBytesSect.CopyTo(fitSpan);
                return;
            }
        }

        throw new Exception("Empty records in FIT not found");
    }

    public int CombineMicrocodes(string inputFile, string mCodesTableFile, string mCodesDirectory, string outputFile)
    {
        var mTableJson = _fileManager.ReadString(mCodesTableFile);
        var mTable = _jsonSerializer.Deserialize<MicrocodesTable>(mTableJson);

        var fitBytes = inputFitFile != null
            ? _fileManager.ReadBytes(inputFitFile)
            : CreateFitForMCodes(mTable.MicrocodeFiles.Length);

        // MPDT
        var tail = new byte[]
            { 0x4D, 0x50, 0x44, 0x54, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00, };
        var payloadSpace = mTable.TargetSize - tail.Length;

        var tableBytes = new byte[mTable.TargetSize];
        Array.Fill(tableBytes, (byte)0xFF);
        var position = 0u;
        foreach (var mFile in mTable.MicrocodeFiles)
        {
            var mBytes = _fileManager.ReadBytes(mFile);
            _logger.LogDebug("Read {count} bytes", mBytes.Length);

            if (mBytes.Length + position > payloadSpace)
            {
                _logger.LogError("Try write {try} bytes but free space is {free}", mBytes.Length,
                    payloadSpace - position);
                throw new Exception("No space on payload section");
            }

            // copy
            Array.Copy(mBytes, 0, tableBytes, position, mBytes.Length);
            var fwStart = position + mTable.SectionBaseAddress;
            var fwEnd = fwStart + mBytes.Length;
            _logger.LogInformation("Place {file} in range 0x{from:X8}-0x{to:X8}", mFile, fwStart, fwEnd);

            // inject to fit
            InjectMCode2Fit(fitBytes, fwStart);

            // ff
            position += (uint)mBytes.Length;
        }

        _logger.LogInformation("Free space at end: 0x{count:X8}", payloadSpace - position);
        Array.Copy(tail, 0, tableBytes, payloadSpace, tail.Length);

        _fileManager.Write(tableBytes, outputFile, true);
        if (outputFitFile != null)
        {
            _fileManager.Write(fitBytes.ToArray(), outputFitFile, true);
        }

        return 0;
    }
}

public class MicrocodesTable
{
    [JsonConverter(typeof(HexConverter2<int>))]
    public required int TargetSize { get; set; } = -1;

    public required string[] MicrocodeFiles { get; set; }

    [JsonConverter(typeof(HexConverter2<uint>))]
    public uint SectionBaseAddress { get; set; } = 0;
}

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
            WriteEntry(fitTable.Entries[i], fitBytes.AsSpan(fitHeadStart + i * FitEntrySize));
        }

        return fitBytes;
    }

    public FitTable Read(byte[] fitBytes)
    {
        var fitHeadMarker = "_FIT_   "u8;

        var begin = -1;
        for (int i = 0; i < fitBytes.Length; i++)
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
        var headEntry = ReadEntry(fitBytes.AsSpan(begin, FitEntrySize));
        if (headEntry.Type != FitEntryType.FitHeaderEntry)
        {
            throw new Exception($"Expected first entry is \"FIT Header Entry\" but read \"{headEntry.Type}\"");
        }

        var tailGarbage = fitBytes.AsSpan(begin + (int)(headEntry.Size * FitEntrySize)).ToArray();

        var fitTable = new FitTable
        {
            HeadGarbage = headGarbage,
            Entries = [headEntry],
            TailGarbage = tailGarbage,
        };

        for (int i = 0; i < headEntry.Size; i++)
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

        var size = BinaryPrimitives.ReadUInt32LittleEndian(entryBytes.Slice(pos, 4)) >> 8;
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

        BinaryPrimitives.WriteUInt32LittleEndian(entryBytes.Slice(pos, 4), entry.Size << 8);
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

public class FitTable
{
    public byte[] HeadGarbage { get; set; } = [];
    public List<FitEntry> Entries { get; set; } = [];
    public byte[] TailGarbage { get; set; } = [];
}

public class FitEntry
{
    /// <summary>
    /// 7:0 Address
    /// </summary>
    public ulong Address { get; set; }

    /// <summary>
    /// 10:8 Size
    /// </summary>
    public uint Size { get; set; }

    /// <summary>
    /// 11 Reserved
    /// </summary>
    public byte Reserved { get; set; }

    /// <summary>
    /// 13:12 Version
    /// </summary>
    public ushort Version { get; set; }

    /// <summary>
    /// 14 Bit 7 - C_V
    /// </summary>
    public bool ChecksumValidate { get; set; }

    /// <summary>
    /// 14 Bits 6:0 - Type
    /// </summary>
    public FitEntryType Type { get; set; }

    /// <summary>
    /// 15 Chksum
    /// </summary>
    public byte Checksum { get; set; }
}

public enum FitEntryType : byte
{
    /// <summary>
    /// FIT Header Entry
    /// </summary>
    FitHeaderEntry = 0x00,

    /// <summary>
    /// Microcode Update Entry
    /// </summary>
    MicrocodeUpdateEntry = 0x01,

    /// <summary>
    /// Startup AC Module Entry
    /// </summary>
    StartupAcModuleEntry = 0x02,

    /// <summary>
    /// Diagnostic AC Module Entry
    /// </summary>
    DiagnosticAcModuleEntry = 0x03,

    // 0x04 - 0x06 Intel Reserved

    /// <summary>
    /// BIOS Startup Module Entry
    /// </summary>
    BiosStartupModuleEntry = 0x07,

    /// <summary>
    /// TPM Policy Record
    /// </summary>
    TpmPolicyRecord = 0x08,

    /// <summary>
    /// BIOS Policy Record
    /// </summary>
    BiosPolicyRecord = 0x09,

    /// <summary>
    /// TXT Policy Record
    /// </summary>
    TxtPolicyRecord = 0x0A,

    /// <summary>
    /// Key Manifest Record
    /// </summary>
    KeyManifestRecord = 0x0B,

    /// <summary>
    /// Boot Policy Manifest
    /// </summary>
    BootPolicyManifest = 0x0C,

    // 0x0D - 0x0F Intel Reserved

    /// <summary>
    /// CSE Secure Boot
    /// </summary>
    CseSecureBoot = 0x10,

    // 0x11 - 0x2C Intel Reserved

    /// <summary>
    /// Feature Policy Delivery Record
    /// </summary>
    FeaturePolicyDeliveryRecord = 0x2D,

    // 0x2E Intel Reserved

    /// <summary>
    /// JMP $ Debug Policy
    /// </summary>
    JmpDebugPolicy = 0x2F,

    // 0x30 - 0x70 Reserved for Platform Manufacturer Use
    // 0x71 - 0x7E Intel Reserved

    /// <summary>
    /// Unused Entry (skip)
    /// </summary>
    UnusedEntry = 0x7F
}
