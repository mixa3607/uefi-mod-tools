using ArkProjects.UefiModTools.Services.ManifestVer;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Services.ManifestVer;

public class ManifestVersionVerifierTests
{
    [Fact]
    public void VerifyAcceptsSupportedManifest()
    {
        var manifest = new TestManifest { Type = "test", Version = 2 };

        CreateVerifier().Verify(manifest, "test manifest", "test", ignoreVersions: false, 1, 2);
    }

    [Fact]
    public void VerifyRejectsDifferentTypeEvenWhenIgnoringVersions()
    {
        var manifest = new TestManifest { Type = "other", Version = 2 };

        var error = Assert.Throws<ArgumentException>(() =>
            CreateVerifier().Verify(manifest, "test manifest", "test", ignoreVersions: true, 2));

        Assert.Contains("cannot ignore a different document type", error.Message);
    }

    [Fact]
    public void VerifyAllowsUnsupportedVersionWhenRequested()
    {
        var manifest = new TestManifest { Type = "test", Version = 3 };

        CreateVerifier().Verify(manifest, "test manifest", "test", ignoreVersions: true, 2);
    }

    [Fact]
    public void VerifyRejectsUnsupportedVersion()
    {
        var manifest = new TestManifest { Type = "test", Version = 3 };

        var error = Assert.Throws<ArgumentException>(() =>
            CreateVerifier().Verify(manifest, "test manifest", "test", ignoreVersions: false, 2));

        Assert.Contains("manifest version 2", error.Message);
    }

    private static ManifestVersionVerifier CreateVerifier() => new(NullLogger<ManifestVersionVerifier>.Instance);

    private class TestManifest : IVersionedManifest
    {
        public int Version { get; init; }
        public string Type { get; init; } = string.Empty;
    }
}
