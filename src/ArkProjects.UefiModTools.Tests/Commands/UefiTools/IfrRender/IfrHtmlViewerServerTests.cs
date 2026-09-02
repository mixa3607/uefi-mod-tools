using System.Net;
using ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrRender;

public class IfrHtmlViewerServerTests
{
    [Theory]
    [InlineData("127.0.0.1:4060", "127.0.0.1", 4060)]
    [InlineData("localhost:80", "127.0.0.1", 80)]
    [InlineData("::1:4060", "::1", 4060)]
    public void ParseEndpointAcceptsLoopbackAddresses(string input, string expectedAddress, int expectedPort)
    {
        var endpoint = UefiEditorServer.ParseEndpoint(input);

        Assert.Equal(IPAddress.Parse(expectedAddress), endpoint.Address);
        Assert.Equal(expectedPort, endpoint.Port);
    }

    [Theory]
    [InlineData("localhost")]
    [InlineData("127.0.0.1")]
    [InlineData("127.0.0.1:0")]
    [InlineData("0.0.0.0:4060")]
    [InlineData("example.com:4060")]
    public void ParseEndpointRejectsInvalidOrNonLoopbackAddresses(string input)
    {
        Assert.Throws<ArgumentException>(() => UefiEditorServer.ParseEndpoint(input));
    }
}
