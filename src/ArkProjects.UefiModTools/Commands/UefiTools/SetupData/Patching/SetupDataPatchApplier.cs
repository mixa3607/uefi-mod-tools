using Microsoft.Extensions.Logging;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Format;
using ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Mapping;

namespace ArkProjects.UefiModTools.Commands.UefiTools.SetupData.Patching;

public class SetupDataPatchApplier
{
    private readonly ILogger<SetupDataPatchApplier> _logger;

    public SetupDataPatchApplier(ILogger<SetupDataPatchApplier> logger)
    {
        _logger = logger;
    }

    public void Apply(byte[] setupData, IReadOnlyList<SetupDataQuestionMapping> questions, IReadOnlyList<SetupDataQuestionPatch> patches)
    {
        var questionsById = questions.ToDictionary(x => x.Id, StringComparer.Ordinal);
        foreach (var patch in patches)
        {
            if (!questionsById.TryGetValue(patch.Id, out var mappedQuestion))
                throw new ArgumentException($"No mapped SetupData question has id '{patch.Id}'", nameof(patches));

            var range = ValidateRangeAndPattern(setupData, mappedQuestion);
            var question = setupData.AsSpan(range.Offset, range.Length);
            var changed = false;

            if (patch.AccessLevel is { } accessLevel)
            {
                question[AmiSetupDataQuestionOffset.AccessLevel] = accessLevel;
                changed = true;
            }
            if (patch.Failsafe is { } failsafe)
            {
                question[AmiSetupDataQuestionOffset.Failsafe] = failsafe;
                changed = true;
            }
            if (patch.Optimal is { } optimal)
            {
                question[AmiSetupDataQuestionOffset.Optimal] = optimal;
                changed = true;
            }

            if (changed)
                _logger.LogInformation("Patched SetupData question {id} at offset 0x{offset:X}", patch.Id, range.Offset);
        }
    }

    private static (int Offset, int Length) ValidateRangeAndPattern(byte[] setupData, SetupDataQuestionMapping mappedQuestion)
    {
        if (mappedQuestion.BeginAddress < 0 || mappedQuestion.EndAddress < mappedQuestion.BeginAddress ||
            mappedQuestion.EndAddress > setupData.Length)
        {
            throw new InvalidDataException($"SetupData question '{mappedQuestion.Id}' is outside the input");
        }

        var pattern = new AmiSetupDataQuestionPattern(
            mappedQuestion.Question.QuestionId,
            mappedQuestion.Question.HelpStringId,
            mappedQuestion.Question.PromptStringId);
        var range = (Offset: mappedQuestion.BeginAddress, Length: mappedQuestion.EndAddress - mappedQuestion.BeginAddress);
        if (!pattern.IsMatch(setupData.AsSpan(range.Offset, range.Length)))
            throw new InvalidDataException($"SetupData question '{mappedQuestion.Id}' does not match the map data");

        return range;
    }
}
