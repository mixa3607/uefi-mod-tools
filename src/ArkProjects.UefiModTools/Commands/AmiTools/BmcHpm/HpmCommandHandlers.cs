using System.Text;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcHpm;

// function doComponentIdVersions(cid) {
//   switch (cid) {
//     case 1:
//       HPM_COMPONENT_DATA_VERSION_ID.push(0);
//       break;
//     case 2:
//       HPM_COMPONENT_DATA_VERSION_ID.push(1);
//       break;
//     case 4:
//       HPM_COMPONENT_DATA_VERSION_ID.push(2);
//       break;
//   }
// }

public class HpmCommandHandlers
{
    private const ushort OemHeaderLen = 16;
    private const ushort HpmHeaderLen = 34;

    // ebit = sbit + 31 + 16; //16 bytes added for OEM Header
    private const ushort HpmUpgradeComponentLen = OemHeaderLen + 31;
    private const ushort HpmUpgradePayloadLen = 47;
    private const ushort HpmActionLen = 3;

    private readonly ILogger<HpmCommandHandlers> _logger;

    public HpmCommandHandlers(ILogger<HpmCommandHandlers> logger)
    {
        _logger = logger;
    }

    // ====== hpm header 0 - 33         // var headerEndOffBit = 34;
    // str[4]:  0       - signature     // "HPM.1"
    // byte:    20      - components    // NoOfComponents = result.substring(40, 42);
    // ushort:  32 - 33 - oem data len  // var totalOEMLength = result.substring(64, 68)
    private void WriteHpmHeader(BinaryWriter writer, byte componentsCount)
    {
        var begin = (int)writer.Seek(0, SeekOrigin.Current);
        writer.Write(new byte[HpmHeaderLen]);

        writer.Seek(begin + 0, SeekOrigin.Begin);
        writer.Write(Encoding.ASCII.GetBytes("HPM.1"));

        writer.Seek(begin + 20, SeekOrigin.Begin);
        writer.Write(componentsCount);

        writer.Seek(begin + 32, SeekOrigin.Begin);
        writer.Write(EndianUtilities.Swap(OemHeaderLen));

        writer.Seek(0, SeekOrigin.End);
        writer.Flush();
    }

    // ====== oem header 0 - 15         // TOTAL_HPM_SIZE = file.size - 16;
    private void WriteOemHeader(BinaryWriter writer)
    {
        writer.Write(new byte[OemHeaderLen]);
        writer.Flush();
    }

    // ====== hpm action 0 - 2
    // byte: 0          - action type   // var upgradeActionType = result.substring(0, 2)
    // byte: 0          - component id  // var componentId = parseInt(result.substring(2, 4));
    private void WriteHpmAction(BinaryWriter writer, HpmActionType action, HpmComponentType type)
    {
        var begin = (int)writer.Seek(0, SeekOrigin.Current);
        writer.Write(new byte[HpmActionLen]);

        writer.Seek(begin + 0, SeekOrigin.Begin);
        writer.Write((byte)action);

        writer.Seek(begin + 1, SeekOrigin.Begin);
        writer.Write((byte)type);

        writer.Seek(0, SeekOrigin.End);
        writer.Flush();
    }

    // ====== hpm upgrade payload 0 - 47
    // byte[6]:  0      - component version // var upgradeActionType = result.substring(0, 2)
    // byte:     0      - major
    // byte:     1      - minor
    // uint:     4      - patch
    // str[21]:  6      - component name    // var componentName = hex2a(result.substring(12, 54)); //Component Desc/Name
    // uint:     27     - component len     // var componentLength = result.substring(54, 62); //Component length
    // byte[16]: 31     - OEM header        // var oemHeaderLength = result.substring(62, 94); //OEM Header Length 16 bytes
    // str[4]:   31     - OEM signature "OEM\x00" // var oemsig = hex2a(result.substring(62, 70)); //OEM Signature; 4 bytes
    // uint:     35     - OEM flash size    // var oemsectionflashvalue = result.substring(70, 78); //OEM Signature;  4 bytes
    private void WriteHpmUpgrade(BinaryWriter writer, Version version, string name, uint len)
    {
        var begin = (int)writer.Seek(0, SeekOrigin.Current);
        writer.Write(new byte[HpmUpgradePayloadLen]);

        writer.Seek(begin + 0, SeekOrigin.Begin);
        writer.Write((byte)version.Major);
        writer.Write((byte)version.Minor);
        writer.Write((uint)version.Build);

        writer.Seek(begin + 6, SeekOrigin.Begin);
        writer.Write(Encoding.ASCII.GetBytes(name));

        writer.Seek(begin + 27, SeekOrigin.Begin);
        writer.Write(len + 16);

        writer.Seek(begin + 31, SeekOrigin.Begin);
        //writer.Write(Encoding.ASCII.GetBytes("OEM"));
        writer.Write(Encoding.ASCII.GetBytes("AAAA"));

        writer.Seek(begin + 35, SeekOrigin.Begin);
        writer.Write(0u);

        writer.Seek(0, SeekOrigin.End);
        writer.Flush();
    }


    public int BuildHpm()
    {
        var biosFile = CommandHelpers.ReadBytes("./test-files/IMB760_BIOS_mixa3607_F8CC6E033BF2_reset.bin", _logger);
        var memStream = new MemoryStream();
        var memWriter = new BinaryWriter(memStream);

        WriteHpmHeader(memWriter, 3);
        WriteOemHeader(memWriter);
        memWriter.Write((byte)0x00); // maybe pad byte?

        WriteHpmAction(memWriter, HpmActionType.UploadComponents, HpmComponentType.Bios);
        WriteHpmUpgrade(memWriter, new Version(1, 1, 1), "BIOS", (uint)biosFile.Length);
        memWriter.Write(biosFile);

        //WriteHpmAction(memWriter, HpmActionType.UploadComponents, HpmComponentType.Bios);
        //WriteHpmUpgrade(memWriter, new Version(1, 1, 1), "STUB 1", 0);
        //
        //WriteHpmAction(memWriter, HpmActionType.UploadComponents, HpmComponentType.Bios);
        //WriteHpmUpgrade(memWriter, new Version(1, 1, 1), "STUB 2", 0);

        memWriter.Write((byte)0xFF);

        memWriter.Flush();
        CommandHelpers.WriteResult(memStream.ToArray(),
            "./test-files/IMB760_BIOS_mixa3607_F8CC6E033BF2_reset.hpm",
            true, _logger);
        return 0;
    }
}
