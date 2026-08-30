using ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrRender;

public class IfrHtmlViewerRendererTests
{
    [Fact]
    public void RenderEmbedsDocumentAndInteractiveViewer()
    {
        var html = new IfrHtmlViewerRenderer().Render("{\"Formsets\":[]}");

        Assert.Contains("<script id=\"ifr-data\" type=\"application/json\">{\"Formsets\":[]}</script>", html);
        Assert.Contains("JSON.parse(document.getElementById('ifr-data').textContent)", html);
        Assert.Contains("Search prompt, QuestionId, VarOffset, condition", html);
        Assert.Contains("always true", html);
        Assert.Contains("The nested items are affected only while this condition is true", html);
    }
}
