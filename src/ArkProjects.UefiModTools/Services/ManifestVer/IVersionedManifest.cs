namespace ArkProjects.UefiModTools.Services.ManifestVer;

public interface IVersionedManifest
{
    int Version { get; }
    string Type { get; }
}
