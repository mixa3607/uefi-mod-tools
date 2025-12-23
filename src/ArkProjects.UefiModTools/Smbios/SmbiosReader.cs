using System.Text;

namespace ArkProjects.UefiModTools;

public class SmbiosReader
{
    public SmbiosDump Read(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var smbios = new SmbiosDump() { Length = (int)stream.Length };

        while (true)
        {
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
        var type = reader.ReadByte();
        var len = reader.ReadByte();
        var handler = reader.ReadUInt16();
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
            stack.Push(reader.ReadByte());
        } while (stack.Peek() != 0);

        stack.Pop();
        return Encoding.ASCII.GetString(stack.Reverse().ToArray());
    }
}
