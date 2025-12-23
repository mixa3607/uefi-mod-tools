using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;

namespace ArkProjects.UefiModTools.Commands;

public static class CommandHelpers
{
    public static void WriteResult(string dataString, string output, bool overrideOutput, ILogger logger)
    {
        var dataBytes = Encoding.UTF8.GetBytes(dataString + Environment.NewLine);
        WriteResult(dataBytes, output, overrideOutput, logger);
    }

    public static void WriteResult(byte[] dataBytes, string output, bool overrideOutput, ILogger logger)
    {
        logger.LogInformation("Save output to {out}", output);

        if (output == "-")
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
