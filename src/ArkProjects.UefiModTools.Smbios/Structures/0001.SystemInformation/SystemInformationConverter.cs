namespace ArkProjects.UefiModTools.Smbios.Structures;

public class SystemInformationConverter : ISmbiosStructureReader, ISmbiosStructureWriter
{
    public SmbiosStructureType AllowedStructureType => SmbiosStructureType.SystemInformation;

    public ISmbiosStructure Read(SmbiosRawStructure structure)
    {
        using var memStream = new MemoryStream(structure.Body);
        using var reader = new BinaryReader(memStream);

        var manufacturer = structure.Strings[reader.ReadByte() - 1];
        var productName = structure.Strings[reader.ReadByte() - 1];
        var version = structure.Strings[reader.ReadByte() - 1];
        var serialNumber = structure.Strings[reader.ReadByte() - 1];
        var uuid = new Guid(reader.ReadBytes(16));
        var wakeUpType = (SystemWakeupType)reader.ReadByte();
        var skuNumber = structure.Strings[reader.ReadByte() - 1];
        var family = structure.Strings[reader.ReadByte() - 1];

        var body = new SystemInformationStructure()
        {
            StructureHandle = structure.StructureHandle,
            //
            Manufacturer = manufacturer,
            ProductName = productName,
            Version = version,
            SerialNumber = serialNumber,
            Uuid = uuid,
            WakeUpType = wakeUpType,
            SkuNumber = skuNumber,
            Family = family,
        };
        return body;
    }

    public SmbiosRawStructure Write(ISmbiosStructure body) => Write((SystemInformationStructure)body);

    public SmbiosRawStructure Write(SystemInformationStructure body)
    {
        using var memStream = new MemoryStream();
        using var writer = new BinaryWriter(memStream);
        var strings = new List<string>();

        strings.Add(body.Manufacturer);
        writer.Write((byte)strings.Count);
        //
        strings.Add(body.ProductName);
        writer.Write((byte)strings.Count);
        //
        strings.Add(body.Version);
        writer.Write((byte)strings.Count);
        //
        strings.Add(body.SerialNumber);
        writer.Write((byte)strings.Count);
        //
        writer.Write(body.Uuid.ToByteArray());
        writer.Write((byte)body.WakeUpType);
        //
        strings.Add(body.SkuNumber);
        writer.Write((byte)strings.Count);
        //
        strings.Add(body.Family);
        writer.Write((byte)strings.Count);

        writer.Flush();

        return new SmbiosRawStructure()
        {
            Strings = strings.ToArray(),
            StructureHandle = body.StructureHandle,
            StructureType = body.StructureType,
            Body = memStream.ToArray()
        };
    }
}
