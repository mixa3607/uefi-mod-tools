using System.Text;

namespace ArkProjects.UefiModTools.Smbios;

public class SmbiosWriter
{
    public void Write(SmbiosDump smbios, Stream stream)
    {
        using var writer = new BinaryWriter(stream);

        foreach (var structure in smbios.Structures)
        {
            WriteStructure(writer, structure);
        }

        writer.Flush();
        var bytesToFill = smbios.Length - writer.BaseStream.Position;
        if (bytesToFill < 0)
        {
            throw new Exception(
                $"All structures use {writer.BaseStream.Position} bytes but max len is {smbios.Length}! " +
                $"Try use smaller data");
        }

        for (int i = 0; i < bytesToFill; i++)
        {
            writer.Write((byte)0xFF);
        }
    }

    private void WriteStructure(BinaryWriter writer, SmbiosRawStructure structure)
    {
        writer.Write((byte)structure.StructureType);
        writer.Write(structure.StructureLength);
        writer.Write(structure.StructureHandle);
        writer.Write(structure.Body);

        if (structure.Strings.Length == 0)
        {
            writer.Write((byte)0);
            writer.Write((byte)0);
        }
        else
        {
            foreach (var str in structure.Strings)
            {
                var bytes = Encoding.ASCII.GetBytes(str);
                writer.Write(bytes);
                writer.Write((byte)0);
            }

            writer.Write((byte)0);
        }
    }
}
