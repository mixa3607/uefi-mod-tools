using ArkProjects.UefiModTools.Commands.UefiTools.Ifr;
using ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;
using ArkProjects.UefiModTools.Services;
using ArkProjects.UefiModTools.Services.Serialization;

namespace ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;

public class UefiEditorCommandHandlers
{
    private readonly ICommandFileManager _fileManager;
    private readonly ISerializationService _serializer;
    private readonly UefiEditorHtmlRenderer _htmlRenderer;
    private readonly UefiEditorServer _server;

    public UefiEditorCommandHandlers(ICommandFileManager fileManager, ISerializationService serializer,
        UefiEditorHtmlRenderer htmlRenderer, UefiEditorServer server)
    {
        _fileManager = fileManager;
        _serializer = serializer;
        _htmlRenderer = htmlRenderer;
        _server = server;
    }

    public int Serve(string inputFile, string address)
    {
        var document = _serializer.Deserialize<IfrDocument>(_fileManager.ReadString(inputFile), SerializationFormat.Auto);
        if (document.Type != IfrDocument.SupportedType || document.Version != IfrDocument.SupportedVersion ||
            !IsSha256(document.IfrSha256))
            throw new ArgumentException("Input is not a supported AMI IFR render document.", nameof(inputFile));

        return _server.Serve(_htmlRenderer.Render(_serializer.Serialize(document, SerializationFormat.Json)), address);
    }

    private static bool IsSha256(string value) => value.Length == 64 && value.All(Uri.IsHexDigit);
}
