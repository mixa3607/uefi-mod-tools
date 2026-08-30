namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSct;

public class IfrSctPatches
{
    public int Version { get; set; } = 1;
    public List<IfrSctDisableSuppressIfPatch> SuppressIfPatches { get; set; } = [];
}

public class IfrSctDisableSuppressIfPatch
{
    public bool Disable { get; set; }
    public int Offset { get; set; }
}
