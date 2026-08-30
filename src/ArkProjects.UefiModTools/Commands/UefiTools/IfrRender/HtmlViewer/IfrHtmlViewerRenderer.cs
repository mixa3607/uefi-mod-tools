using System.Reflection;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender.HtmlViewer;

public class IfrHtmlViewerRenderer
{
    private const string DataPlaceholder = "{{IFR_DATA}}";
    private static readonly Lazy<string> Template = new(() => ReadResource("IfrHtmlViewer.Template"));
    private static readonly Lazy<string> Styles = new(() => ReadResource("IfrHtmlViewer.Styles"));
    private static readonly Lazy<string> Script = new(() => ReadResource("IfrHtmlViewer.Script"));

    public string Render(string renderedJson) => Template.Value
        .Replace("<!-- IFR_VIEWER_STYLES -->", Styles.Value)
        .Replace("<!-- IFR_VIEWER_SCRIPT -->", Script.Value)
        .Replace(DataPlaceholder, renderedJson);

    private static string ReadResource(string name)
    {
        var assembly = typeof(IfrHtmlViewerRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded IFR viewer resource '{name}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
