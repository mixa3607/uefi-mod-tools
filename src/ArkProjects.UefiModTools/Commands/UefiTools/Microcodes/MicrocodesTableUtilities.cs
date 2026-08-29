namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

internal static class MicrocodesTableUtilities
{
    public static (int Start, int End) GetUsableRange(MicrocodesTable table, int inputLength)
    {
        var end = table.UsableEnd < 0 ? inputLength : table.UsableEnd;
        if (table.UsableStart < 0 || table.UsableStart > end || end > inputLength)
            throw new ArgumentException("Microcode usable range is outside the input file");

        return (table.UsableStart, end);
    }
}
