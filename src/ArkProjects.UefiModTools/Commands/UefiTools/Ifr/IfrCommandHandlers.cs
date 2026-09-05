using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public class IfrCommandHandlers
{
    private readonly ILogger<IfrCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly ISerializationService _serializer;
    private readonly IfrDocumentRenderer _documentRenderer;

    public IfrCommandHandlers(ILogger<IfrCommandHandlers> logger, ICommandFileManager fileManager,
        ISerializationService serializer, IfrDocumentRenderer documentRenderer)
    {
        _logger = logger;
        _fileManager = fileManager;
        _serializer = serializer;
        _documentRenderer = documentRenderer;
    }

    public int Render(string ifrFile, string outputFile, SerializationFormat outputFormat)
    {
        var ifr = _serializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile), SerializationFormat.Auto);
        var document = new IfrDocument
        {
            Version = IfrDocument.SupportedVersion,
            Type = IfrDocument.SupportedType,
            IfrSha256 = ifr.InputSha256,
            Formsets = _documentRenderer.RenderFormsets(ifr.Operations),
        };
        _logger.LogInformation("Rendered {formsetCount} formsets from {operationCount} IFR operations", document.Formsets.Count, ifr.Operations.Count);
        _fileManager.Write(_serializer.Serialize(document, outputFormat), outputFile, true);
        return 0;
    }
}
