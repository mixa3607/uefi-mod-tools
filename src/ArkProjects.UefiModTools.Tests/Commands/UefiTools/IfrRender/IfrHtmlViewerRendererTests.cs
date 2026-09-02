using ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrRender;

public class IfrHtmlViewerRendererTests
{
    [Fact]
    public void RenderEmbedsDocumentAndInteractiveViewer()
    {
        var html = new UefiEditorHtmlRenderer().Render("{\"Formsets\":[]}");

        Assert.Contains("<div id=\"root\"></div>", html);
        Assert.Contains("<script id=\"ifr-data\" type=\"application/json\">{\"Formsets\":[]}</script>", html);
        Assert.Contains("JSON.parse", html);
        Assert.Contains("ifr-data", html);
        Assert.Contains("@media", html);
        Assert.DoesNotContain("UEFI_EDITOR_", html);
        Assert.DoesNotContain("<script src=", html);
    }
}
