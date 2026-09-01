using System.Text;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrBiosDefaults;

public static class BinaryReaderExtensions
{
    extension(BinaryReader reader)
    {
        public string ReadNullTerminatedString(long endPosition)
        {
            var chars = new List<byte>();
            while (reader.BaseStream.Position < endPosition)
            {
                var readByte = reader.ReadByte();
                if (readByte == 0x00)
                    return Encoding.ASCII.GetString(chars.ToArray());

                chars.Add(readByte);
            }

            throw new InvalidDataException("NVAR record name is not null-terminated");
        }

        public int ReadUInt24()
        {
            return reader.ReadByte()
                   | (reader.ReadByte() << 8)
                   | (reader.ReadByte() << 16);
        }
    }
}
