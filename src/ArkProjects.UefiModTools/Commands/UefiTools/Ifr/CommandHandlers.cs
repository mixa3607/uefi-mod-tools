using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;

    public CommandHandlers(ILogger<CommandHandlers> logger)
    {
        _logger = logger;
    }

    public int ExtractSetupData(string inputFile, string ifrFile, string outputFile)
    {
        _logger.LogError("IFR SetupData extraction is not implemented");
        return 1;
    }

    public int PatchSetupData(string inputFile, string patchFile, string outputFile)
    {
        _logger.LogError("IFR SetupData patching is not implemented");
        return 1;
    }

    public int PatchSct(string inputFile, string ifrFile, string patchFile, string outputFile)
    {
        _logger.LogError("IFR Platform_setup.sct patching is not implemented");
        return 1;
    }
}
