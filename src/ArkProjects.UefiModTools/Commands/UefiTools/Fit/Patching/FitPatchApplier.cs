using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Mapping;
using ArkProjects.UefiModTools.Commands.UefiTools.Fit.Parser;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit.Patching;

public class FitPatchApplier
{
    public void Apply(FitTable fitTable, FitMapDocument map, IReadOnlyList<FitPatchOperation> operations)
    {
        var entriesById = map.Entries.ToDictionary(entry => entry.Id, StringComparer.Ordinal);

        foreach (var operation in operations)
        {
            if (!entriesById.TryGetValue(operation.Id, out var mapping))
                throw new ArgumentException($"FIT map does not contain entry '{operation.Id}'", nameof(operations));
            if (mapping.Index == 0)
                throw new ArgumentException("The FIT header entry cannot be changed", nameof(operations));

            fitTable.Entries[mapping.Index] = operation.Kind switch
            {
                FitPatchOperationKind.Clear => new FitEntry
                {
                    Type = FitEntryType.UnusedEntry,
                    Checksum = 0,
                    ChecksumValidate = false,
                    Version = 0x00000100,
                    Size = 0x00000000,
                    Address = 0xFFFFFFFF,
                    Reserved = 0
                },
                FitPatchOperationKind.Write when operation.Entry is not null => operation.Entry,
                FitPatchOperationKind.Write => throw new ArgumentException(
                    $"Write operation for '{operation.Id}' requires an entry value", nameof(operations)),
                _ => throw new ArgumentException($"Unsupported FIT patch operation '{operation.Kind}'", nameof(operations)),
            };
        }
    }
}
