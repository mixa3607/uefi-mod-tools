using System.Runtime.InteropServices;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

// https://github.com/ya-mouse/bmc-ami/blob/master/genimage/fmh.h
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AmiFlashModuleHeader
{
    // "$MODULE$"
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Signature;

    public byte VerMajor;

    public byte VerMinor;

    public ushort Size;

    public uint AllocatedSize;

    public uint Location;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public byte[] Reserved;

    public byte HeaderChecksum;

    public AmiModuleInfo ModuleInfo;

    // 0x55AA
    public ushort EndSignature;
}
