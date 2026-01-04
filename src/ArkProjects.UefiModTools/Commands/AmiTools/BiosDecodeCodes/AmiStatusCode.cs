namespace ArkProjects.UefiModTools.Commands.AmiTools.BiosDecodeCodes;

public class AmiStatusCode
{
    public byte Value { get; set; }
    public string Phase { get; set; }
    public string Group { get; set; }
    public string Description { get; set; }

    public AmiStatusCode(byte value, string phase, string group, string description)
    {
        Value = value;
        Phase = phase;
        Group = group;
        Description = description;
    }
}
