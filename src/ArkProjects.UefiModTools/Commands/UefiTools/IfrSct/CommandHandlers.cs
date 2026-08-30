using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSct;

public class CommandHandlers
{
    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly IfrSctPatcher _patcher;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, IfrSctPatcher patcher)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _patcher = patcher;
    }

    public int Patch(string inputFile, string ifrFile, string patchFile, string outputFile)
    {
        var sct = _fileManager.ReadBytes(inputFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        var patches = _jsonSerializer.Deserialize<IfrSctPatches>(_fileManager.ReadString(patchFile));
        _logger.LogInformation(
            "Read {sctSize} bytes of {inputFile}, {operationCount} IFR operations, and patch version {patchVersion}",
            sct.Length, inputFile, ifr.Operations.Count, patches.Version);

        _patcher.Apply(sct, ifr.Operations, patches);

        _logger.LogInformation("Writing Platform_setup.sct to {outputFile}", outputFile);
        _fileManager.Write(sct.ToArray(), outputFile, true);
        return 0;
    }
}
