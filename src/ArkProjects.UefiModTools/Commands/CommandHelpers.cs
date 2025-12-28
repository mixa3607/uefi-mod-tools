using Microsoft.Extensions.Logging;
using System.Text;

namespace ArkProjects.UefiModTools.Commands;

public static class CommandHelpers
{
    private static Stream GetReadingStream(string input, ILogger logger)
    {
        if (input.StartsWith("-"))
        {
            logger.LogInformation("Reading data from console");
            return Console.OpenStandardInput();
        }
        else if (File.Exists(input))
        {
            logger.LogInformation("Reading data from file {file}", input);
            return File.OpenRead(input);
        }

        throw new Exception($"File {input} not exist");
    }

    public static string ReadString(string input, Encoding? encoding, ILogger logger)
    {
        using var stream = GetReadingStream(input, logger);
        using var reader = new StreamReader(stream, encoding);
        return reader.ReadToEnd();
    }

    public static byte[] ReadBytes(string input, ILogger logger)
    {
        using var stream = GetReadingStream(input, logger);
        var bytes = new List<byte>();
        var buffer = new byte[2048];
        var read = 0;
        while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            bytes.AddRange(buffer.AsSpan(0, read));
        }

        return bytes.ToArray();
    }

    public static void WriteResult(string dataString, string output, bool overrideOutput, ILogger logger)
    {
        var dataBytes = Encoding.UTF8.GetBytes(dataString + Environment.NewLine);
        WriteResult(dataBytes, output, overrideOutput, logger);
    }

    public static void WriteResult(byte[] dataBytes, string output, bool overrideOutput, ILogger logger)
    {
        logger.LogInformation("Save output to {out}", output);

        if (output.StartsWith("-"))
        {
            using var cout = Console.OpenStandardOutput();
            cout.Write(dataBytes);
            return;
        }

        if (File.Exists(output))
        {
            if (!overrideOutput)
            {
                throw new Exception($"File {output} already exist");
            }

            File.Delete(output);
        }
        else
        {
            var dir = Path.GetDirectoryName(output);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        File.WriteAllBytes(output, dataBytes);
    }
}
