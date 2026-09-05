using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Services.ManifestVer;

public class ManifestVersionVerifier : IManifestVersionVerifier
{
    private readonly ILogger<ManifestVersionVerifier> _logger;

    public ManifestVersionVerifier(ILogger<ManifestVersionVerifier> logger)
    {
        _logger = logger;
    }

    public void Verify(IVersionedManifest manifest, string manifestName, string type, bool ignoreVersions, params int[] supportedVersions)
    {
        if (manifest.Type != type)
            throw new ArgumentException(
                $"Expected {manifestName} manifest type {type}, but got {manifest.Type}. " +
                "--ignore-versions cannot ignore a different document type.", nameof(manifest));

        if (supportedVersions.Contains(manifest.Version))
            return;

        if (ignoreVersions)
        {
            _logger.LogWarning(
                "{name} manifest version {actualVersion} is not the supported version {@supportedVersion}; continuing because --ignore-versions was specified",
                manifestName, manifest.Version, supportedVersions);
            return;
        }

        throw new ArgumentException(
            $"Expected {manifestName} manifest version {string.Join(',', supportedVersions)}, but got {manifest.Version}. " +
            "Use --ignore-versions only when the map schema is known to be compatible.", nameof(manifest));
    }
}
