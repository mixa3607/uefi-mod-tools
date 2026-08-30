using ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrRender.HtmlViewer;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrRender;

public class IfrHtmlViewerRendererTests
{
    [Fact]
    public void RenderEmbedsDocumentAndInteractiveViewer()
    {
        var html = new IfrHtmlViewerRenderer().Render("{\"Formsets\":[]}");

        Assert.Contains("<div id=\"root\"></div>", html);
        Assert.Contains("<script id=\"ifr-data\" type=\"application/json\">{\"Formsets\":[]}</script>", html);
        Assert.Contains("JSON.parse", html);
        Assert.Contains("ifr-data", html);
        Assert.Contains("@media", html);
        Assert.DoesNotContain("IFR_VIEWER_", html);
        Assert.DoesNotContain("<script src=", html);
    }
}
