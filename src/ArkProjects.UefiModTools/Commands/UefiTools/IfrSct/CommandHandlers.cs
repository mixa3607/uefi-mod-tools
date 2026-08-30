using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSct;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
    }

    public int Patch(string inputFile, string ifrFile, string patchFile, string outputFile)
    {
        var sct = _fileManager.ReadBytes(inputFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        var patches = _jsonSerializer.Deserialize<IfrSctPatches>(_fileManager.ReadString(patchFile));
        _logger.LogInformation("Read {sctSize} bytes of Platform_setup.sct, {operationCount} IFR operations, and patch version {patchVersion}",
            sct.Length, ifr.Operations.Count, patches.Version);

        // Apply IfrSctPatches here.

        _logger.LogInformation("Writing Platform_setup.sct to {outputFile}", outputFile);
        _fileManager.Write(sct, outputFile, true);
        return 0;
    }
}
