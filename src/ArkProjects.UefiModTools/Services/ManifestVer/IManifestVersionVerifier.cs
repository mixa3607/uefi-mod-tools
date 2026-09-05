namespace ArkProjects.UefiModTools.Services.ManifestVer;

public interface IManifestVersionVerifier
{
    void Verify(IVersionedManifest manifest, string manifestName, string type, bool ignoreVersions, params int[] supportedVersions);
}
