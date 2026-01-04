using ArkProjects.UefiModTools.Services;
using ConsoleTables;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BiosDecodeCodes;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager)
    {
        _logger = logger;
        _fileManager = fileManager;
    }

    public int PostDecode(string inputFile, string outputFile)
    {
        var hexCodes = _fileManager.ReadString(inputFile)
            .Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var hexStr = string.Join("", hexCodes);
        var bytes = Convert.FromHexString(hexStr);

        var table = new ConsoleTable("Idx", "Code", "Phase", "Group", "Description");
        table.MaxWidth = int.MaxValue;
        for (int i = 0; i < bytes.Length; i++)
        {
            var code = bytes[i];
            var codeInfo = AmiAptioCodes.StatusCodes.FirstOrDefault(x => x.Value == code);
            if (codeInfo != null)
            {
                table.AddRow([i, $"0x{code:X}", codeInfo.Phase, codeInfo.Group, codeInfo.Description]);
            }
            else
            {
                table.AddRow([i, $"0x{code:X}", "-", "-", "-"]);
            }
        }

        _fileManager.Write(table.ToMarkDownString(), outputFile, true);
        return 0;
    }
}
