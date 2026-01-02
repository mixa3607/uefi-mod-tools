namespace ArkProjects.UefiModTools.Commands.AmiTools.BmcHpm;

public enum HpmActionType : byte
{
    BackupComponents = 0x00,
    PrepareComponents = 0x01,
    UploadComponents = 0x02,
}
