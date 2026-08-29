namespace ArkProjects.UefiModTools.Commands.UefiTools.Microcodes;

public class MicrocodesCombiner
{
    public byte[] Combine(byte[] inputBytes, MicrocodesTable table, IReadOnlyList<byte[]> microcodes)
    {
        if (table.MicrocodeFiles.Length != microcodes.Count)
            throw new ArgumentException("Microcode file and payload counts do not match", nameof(microcodes));

        var usableEnd = table.UsableEnd < 0 ? inputBytes.Length : table.UsableEnd;
        if (table.UsableStart < 0 || table.UsableStart > usableEnd || usableEnd > inputBytes.Length)
            throw new ArgumentException("Microcode usable range is outside the input file", nameof(table));

        var position = table.UsableStart;
        foreach (var microcode in microcodes)
        {
            if (microcode.Length > usableEnd - position)
                throw new ArgumentException("No space on payload section", nameof(microcodes));

            microcode.CopyTo(inputBytes, position);
            position = checked(position + microcode.Length);
        }

        return inputBytes;
    }
}
