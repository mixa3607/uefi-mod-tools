using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public class IfrCommandHandlers
{
    private static readonly IfrJsonSerializerContext JsonContext = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    });

    private readonly ILogger<IfrCommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly IfrDocumentRenderer _documentRenderer;

    public IfrCommandHandlers(ILogger<IfrCommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, IfrDocumentRenderer documentRenderer)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _documentRenderer = documentRenderer;
    }

    public int Render(string ifrFile, string outputFile)
    {
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        var document = new IfrDocument
        {
            Version = IfrDocument.SupportedVersion,
            Type = IfrDocument.SupportedType,
            IfrSha256 = ifr.InputSha256,
            Formsets = _documentRenderer.RenderFormsets(ifr.Operations),
        };
        _logger.LogInformation("Rendered {formsetCount} formsets from {operationCount} IFR operations", document.Formsets.Count, ifr.Operations.Count);
        _fileManager.Write(JsonSerializer.Serialize(document, JsonContext.IfrDocument), outputFile, true);
        return 0;
    }
}
