using System.Text;

namespace ArkProjects.UefiModTools.Smbios;

public class SmbiosReader
{
    public SmbiosDump Read(Stream stream)
    {
        if (!stream.CanSeek)
            throw new ArgumentException("SMBIOS input stream must support seeking", nameof(stream));

        using var reader = new BinaryReader(stream);

        var smbios = new SmbiosDump() { Length = (int)stream.Length };

        while (true)
        {
            if (stream.Position >= stream.Length)
                throw new Exception("SMBIOS table ended before the End-of-Table structure");

            var s = ReadStructure(reader);
            smbios.Structures.Add(s);
            // end of table
            if (s.StructureType == SmbiosStructureType.EndOfTable)
            {
                break;
            }
        }

        return smbios;
    }

    static SmbiosRawStructure ReadStructure(BinaryReader reader)
    {
        if (reader.BaseStream.Length - reader.BaseStream.Position < 4)
            throw new Exception("SMBIOS structure header is truncated");

        var type = reader.ReadByte();
        var len = reader.ReadByte();
        var handler = reader.ReadUInt16();
        if (len < 4)
            throw new Exception($"SMBIOS structure type {type} has invalid length {len}");
        if (len - 4 > reader.BaseStream.Length - reader.BaseStream.Position)
            throw new Exception($"SMBIOS structure type {type} body is truncated");

        var body = reader.ReadBytes(len - 4);
        var strings = new List<string>();

        while (true)
        {
            var str = ReadString(reader);
            // check end of struct
            if (strings.Count == 0 && str == "")
            {
                str = ReadString(reader);
                if (str == "")
                {
                    break;
                }
                else
                {
                    throw new Exception($"Bad data. Expected \"\" byte but read \"{str}\"");
                }
            }

            // end of strings enumeration
            if (strings.Count > 0 && str == "")
            {
                break;
            }

            strings.Add(str);
        }

        return new SmbiosRawStructure()
        {
            StructureType = (SmbiosStructureType)type,
            StructureHandle = handler,
            Body = body,
            Strings = strings.ToArray()
        };
    }

    static string ReadString(BinaryReader reader)
    {
        var stack = new Stack<byte>();
        do
        {
            if (reader.BaseStream.Position >= reader.BaseStream.Length)
                throw new Exception("SMBIOS string-set is truncated");
            stack.Push(reader.ReadByte());
        } while (stack.Peek() != 0);

        stack.Pop();
        return Encoding.ASCII.GetString(stack.Reverse().ToArray());
    }
}
