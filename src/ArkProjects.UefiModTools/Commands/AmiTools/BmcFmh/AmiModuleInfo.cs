using System.Runtime.InteropServices;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcFmh;

// https://github.com/ya-mouse/bmc-ami/blob/master/genimage/fmh.h
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct AmiModuleInfo
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Name;

    public byte VerMajor;
    public byte VerMinor;

    public ushort Type;

    public uint Location;
    public uint Size;

    public ushort Flags;

    public uint LoadAddress;
    public uint Checksum;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public byte[] Reserved;
}
