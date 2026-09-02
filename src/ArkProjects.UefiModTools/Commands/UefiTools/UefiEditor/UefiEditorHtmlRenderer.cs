using System.Reflection;

namespace ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;

public class UefiEditorHtmlRenderer
{
    private const string DataPlaceholder = "{{IFR_DATA}}";
    private static readonly Lazy<string> Template = new(() => ReadResource("UefiEditor.Template"));
    private static readonly Lazy<string> Styles = new(() => ReadResource("UefiEditor.Styles"));
    private static readonly Lazy<string> Script = new(() => ReadResource("UefiEditor.Script"));

    public string Render(string renderedJson) => Template.Value
        .Replace("<!-- UEFI_EDITOR_STYLES -->", Styles.Value)
        .Replace("<!-- UEFI_EDITOR_SCRIPT -->", Script.Value)
        .Replace(DataPlaceholder, renderedJson);

    private static string ReadResource(string name)
    {
        var assembly = typeof(UefiEditorHtmlRenderer).Assembly;
        using var stream = assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded IFR viewer resource '{name}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
