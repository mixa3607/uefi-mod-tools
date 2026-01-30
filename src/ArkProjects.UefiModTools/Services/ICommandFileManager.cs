using System.Text;

namespace ArkProjects.UefiModTools.Services;

public interface ICommandFileManager
{
    string ReadString(string input, Encoding? encoding = null);
    byte[] ReadBytes(string input);
    void Write(string dataString, string output, bool overrideOutput = false, Encoding? encoding = null);
    void Write(ReadOnlySpan<byte> dataBytes, string output, bool overrideOutput);
}
