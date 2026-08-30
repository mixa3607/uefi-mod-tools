using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

public class CommandHandlers
{
    private static readonly CommandJsonSerializerContextIfrRender RenderJsonContext = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    });

    private readonly ILogger<CommandHandlers> _logger;
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly IfrTreeRenderer _treeRenderer;
    private readonly SetupDataParser _setupDataParser;
    private readonly IfrHtmlViewerRenderer _htmlViewerRenderer;

    public CommandHandlers(ILogger<CommandHandlers> logger, ICommandFileManager fileManager,
        IJsonSerializationService jsonSerializer, IfrTreeRenderer treeRenderer, SetupDataParser setupDataParser,
        IfrHtmlViewerRenderer htmlViewerRenderer)
    {
        _logger = logger;
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _treeRenderer = treeRenderer;
        _setupDataParser = setupDataParser;
        _htmlViewerRenderer = htmlViewerRenderer;
    }

    public int Render(string inputFile, string setupDataFile, string ifrFile, string format, string outputFile)
    {
        var sct = _fileManager.ReadBytes(inputFile);
        var setupData = _fileManager.ReadBytes(setupDataFile);
        var ifr = _jsonSerializer.Deserialize<IfrJsonDocument>(_fileManager.ReadString(ifrFile));
        _logger.LogInformation("Read {sctSize} bytes of Platform_setup.sct, {setupDataSize} bytes of SetupData, and {operationCount} IFR operations",
            sct.Length, setupData.Length, ifr.Operations.Count);

        if (format is not ("json" or "html" or "ascii-tree"))
        {
            throw new ArgumentException($"Unsupported render format '{format}'. Use json, html, or ascii-tree.", nameof(format));
        }

        if (format == "ascii-tree")
        {
            _logger.LogError("IFR rendering to ascii-tree is not implemented; output {outputFile} was not written", outputFile);
            return 1;
        }

        var setupDataQuestions = _setupDataParser.ExtractAll(ifr.Operations, setupData);
        var rendered = _treeRenderer.Render(ifr.Operations, setupDataQuestions.Questions);
        var renderedJson = JsonSerializer.Serialize(rendered, RenderJsonContext.IfrRenderDocument);
        _logger.LogInformation("Rendered {formsetCount} formsets to {format}", rendered.Formsets.Count, format);
        _fileManager.Write(format == "html" ? _htmlViewerRenderer.Render(renderedJson) : renderedJson, outputFile, true);
        return 0;
    }
}
