using System.Text.Json;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;
using ArkProjects.UefiModTools.Services;

namespace ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;

public class UefiEditorCommandHandlers
{
    private readonly ICommandFileManager _fileManager;
    private readonly IJsonSerializationService _jsonSerializer;
    private readonly UefiEditorHtmlRenderer _htmlRenderer;
    private readonly UefiEditorServer _server;

    public UefiEditorCommandHandlers(ICommandFileManager fileManager, IJsonSerializationService jsonSerializer,
        UefiEditorHtmlRenderer htmlRenderer, UefiEditorServer server)
    {
        _fileManager = fileManager;
        _jsonSerializer = jsonSerializer;
        _htmlRenderer = htmlRenderer;
        _server = server;
    }

    public int Serve(string inputFile, string address)
    {
        var document = _jsonSerializer.Deserialize<IfrDocument>(_fileManager.ReadString(inputFile));
        if (document.Type != IfrDocument.SupportedType || document.Version != IfrDocument.SupportedVersion ||
            !IsSha256(document.IfrSha256))
            throw new ArgumentException("Input is not a supported AMI IFR render document.", nameof(inputFile));

        return _server.Serve(_htmlRenderer.Render(JsonSerializer.Serialize(document, IfrJsonSerializerContext.Default.IfrDocument)), address);
    }

    private static bool IsSha256(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);
}
