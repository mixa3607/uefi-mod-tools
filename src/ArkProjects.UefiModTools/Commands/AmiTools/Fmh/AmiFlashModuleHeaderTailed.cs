using System.Runtime.InteropServices;

namespace ArkProjects.UefiModTools.Commands.AmiTools.Fmh;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AmiFlashModuleHeaderTailed
{
    public ushort EndSignature;
    public byte HeaderChecksum;
    public byte Reserved;
    public uint LinkAddress;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Signature;
}