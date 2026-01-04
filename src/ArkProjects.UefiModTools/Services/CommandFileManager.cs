using System.Text;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Services;

public class CommandFileManager : ICommandFileManager
{
    private readonly ILogger<CommandFileManager> _logger;

    public CommandFileManager(ILogger<CommandFileManager> logger)
    {
        _logger = logger;
    }


    public string ReadString(string input, Encoding? encoding = null)
    {
        using var stream = GetReadingStream(input);
        using var reader = new StreamReader(stream, encoding);
        return reader.ReadToEnd();
    }

    public byte[] ReadBytes(string input)
    {
        using var stream = GetReadingStream(input);
        using var memStream = new MemoryStream();
        stream.CopyTo(memStream);
        return memStream.ToArray();
    }

    private Stream GetReadingStream(string input)
    {
        if (input.StartsWith("-"))
        {
            _logger.LogInformation("Reading data from {file}", "console");
            return Console.OpenStandardInput();
        }
        else if (File.Exists(input))
        {
            _logger.LogInformation("Reading data from {file}", input);
            return File.OpenRead(input);
        }

        throw new Exception($"File {input} not exist");
    }

    public void Write(string dataString, string output, bool overrideOutput = false, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;
        var dataBytes = encoding.GetBytes(dataString + Environment.NewLine);
        Write(dataBytes, output, overrideOutput);
    }

    public void Write(byte[] dataBytes, string output, bool overrideOutput)
    {
        if (output.StartsWith("-"))
        {
            _logger.LogInformation("Writing output to {out}", "console");
            using var cout = Console.OpenStandardOutput();
            cout.Write(dataBytes);
            return;
        }

        _logger.LogInformation("Writing output to {out}", output);
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
