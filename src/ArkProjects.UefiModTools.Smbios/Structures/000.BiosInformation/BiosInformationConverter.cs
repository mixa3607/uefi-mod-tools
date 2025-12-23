namespace ArkProjects.UefiModTools.Smbios.Structures;

public class BiosInformationConverter : ISmbiosStructureReader, ISmbiosStructureWriter
{
    private const ulong BytesInKilobyte = 1L * 1024;
    private const ulong BytesInMegabyte = BytesInKilobyte * 1024;
    private const ulong BytesInGigabyte = BytesInMegabyte * 1024;

    public SmbiosStructureType AllowedStructureType => SmbiosStructureType.BiosInformation;

    public ISmbiosStructure Read(SmbiosRawStructure structure)
    {
        using var memStream = new MemoryStream(structure.Body);
        using var reader = new BinaryReader(memStream);

        var vendor = structure.Strings[reader.ReadByte() - 1];
        var version = structure.Strings[reader.ReadByte() - 1];
        var startingAddressSegment = reader.ReadUInt16();
        var releaseDate = structure.Strings[reader.ReadByte() - 1];
        var romSize = (ulong)reader.ReadByte();
        var characteristics = reader.ReadUInt64();
        var characteristicsExtensions = reader.ReadBytes((int)(memStream.Length - memStream.Position - 6));
        var systemBiosMajorRelease = reader.ReadByte();
        var systemBiosMinorRelease = reader.ReadByte();
        var embeddedControllerFirmwareMajorRelease = reader.ReadByte();
        var embeddedControllerFirmwareMinorRelease = reader.ReadByte();
        var extendedRomSize = reader.ReadUInt16();
        if (romSize == 0xFF)
        {
            var romSizeUnit = extendedRomSize >> 14;
            var mul = romSizeUnit switch
            {
                (byte)BiosInformationRomSizeUnitType.Megabytes => BytesInMegabyte,
                (byte)BiosInformationRomSizeUnitType.Gigabytes => BytesInGigabyte,
                _ => throw new Exception($"Unknown Extended BIOS ROM Size Unit {romSizeUnit}")
            };
            romSize = mul * (ulong)(extendedRomSize << 2 >> 2);
        }
        else
        {
            // Size (n) where 64K * (n+1)
            romSize = 64 * 1024 * (romSize + 1);
        }

        var body = new BiosInformationStructure()
        {
            StructureHandle = structure.StructureHandle,
            //
            Vendor = vendor,
            Version = version,
            StartingAddressSegment = startingAddressSegment,
            ReleaseDate = releaseDate,
            RomSize = romSize,
            Characteristics = characteristics,
            CharacteristicsExtensions = characteristicsExtensions,
            SystemBiosMajorRelease = systemBiosMajorRelease,
            SystemBiosMinorRelease = systemBiosMinorRelease,
            EmbeddedControllerFirmwareMajorRelease = embeddedControllerFirmwareMajorRelease,
            EmbeddedControllerFirmwareMinorRelease = embeddedControllerFirmwareMinorRelease,
        };
        return body;
    }

    public SmbiosRawStructure Write(ISmbiosStructure body)
    {
        return Write((BiosInformationStructure)body);
    }


    public SmbiosRawStructure Write(BiosInformationStructure body)
    {
        using var memStream = new MemoryStream();
        using var writer = new BinaryWriter(memStream);
        var strings = new List<string>();

        strings.Add(body.Vendor);
        writer.Write((byte)strings.Count);
        //
        strings.Add(body.Version);
        writer.Write((byte)strings.Count);
        //
        writer.Write(body.StartingAddressSegment);
        //
        strings.Add(body.ReleaseDate);
        writer.Write((byte)strings.Count);
        //
        writer.Write(GetRomSize(body));
        writer.Write(body.Characteristics);
        writer.Write(body.CharacteristicsExtensions);
        writer.Write(body.SystemBiosMajorRelease);
        writer.Write(body.SystemBiosMinorRelease);
        writer.Write(body.EmbeddedControllerFirmwareMajorRelease);
        writer.Write(body.EmbeddedControllerFirmwareMinorRelease);
        writer.Write(GetExtendedRomSize(body));

        writer.Flush();

        return new SmbiosRawStructure()
        {
            Strings = strings.ToArray(),
            StructureHandle = body.StructureHandle,
            StructureType = body.StructureType,
            Body = memStream.ToArray()
        };
    }

    private byte GetRomSize(BiosInformationStructure body)
    {
        var romSizeByte = body.RomSize < (16 * BytesInMegabyte)
            ? (byte)(body.RomSize / 64 / 1024 - 1)
            : (byte)0xFF;
        return romSizeByte;
    }

    private ushort GetExtendedRomSize(BiosInformationStructure body)
    {
        if (GetRomSize(body) < 0xFF)
        {
            return 0;
        }

        if (body.RomSize % BytesInGigabyte == 0)
        {
            var unit = BiosInformationRomSizeUnitType.Gigabytes;
            var units = body.RomSize / BytesInGigabyte;
            var maxUnits = (ulong)(ushort.MaxValue >> 2);
            if (units > maxUnits)
            {
                throw new Exception($"Rom size in {unit} ({units}) exceeded max allowed size ({maxUnits})");
            }

            return (ushort)(((ushort)unit << 14) + (ushort)units);
        }
        else if (body.RomSize % BytesInMegabyte == 0)
        {
            var unit = BiosInformationRomSizeUnitType.Megabytes;
            var units = body.RomSize / BytesInMegabyte;
            var maxUnits = (ulong)(ushort.MaxValue >> 2);
            if (units > maxUnits)
            {
                throw new Exception($"Rom size in {unit} ({units}) exceeded max allowed size ({maxUnits})");
            }

            return (ushort)(((ushort)unit << 14) + (ushort)units);
        }
        else
        {
            throw new Exception(
                $"Can not detect rom size unit. Check docs Table 6 – BIOS Information (Type 0) structure[12h]");
        }
    }
}
