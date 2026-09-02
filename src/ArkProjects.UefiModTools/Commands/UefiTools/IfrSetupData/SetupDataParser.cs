using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

public class SetupDataParser
{
    private readonly ILogger<SetupDataParser> _logger;

    public SetupDataParser(ILogger<SetupDataParser> logger)
    {
        _logger = logger;
    }

    public void PatchAll(IReadOnlyList<ExtractedAmiSetupDataQuestion> questions, Memory<byte> setupData)
    {
        var patched = 0;
        var error = 0;
        var all = questions.Count;
        foreach (var question in questions)
        {
            if (question.BeginAddress < 0 || question.EndAddress < question.BeginAddress ||
                question.EndAddress > setupData.Length)
            {
                _logger.LogWarning("{type} question range {beginAddress}-{endAddress} is outside SetupData. Skipping patch",
                    question.Type, question.BeginAddress, question.EndAddress);
                error++;
                continue;
            }

            var pattern = new AmiSetupDataQuestionDataPattern(question.Question.QuestionId,
                question.Question.HelpStringId, question.Question.PromptStringId);
            var matched =
                pattern.IsMatch(
                    setupData.Slice(question.BeginAddress, question.EndAddress - question.BeginAddress).Span);
            if (!matched)
            {
                _logger.LogWarning("{type} question at SetupData offset {offset} does not match the patch data. Skipping patch",
                    question.Type, question.BeginAddress);
                error++;
                continue;
            }

            var origAccessLevel =
                setupData.Slice(question.BeginAddress + AmiSetupDataQuestionOffset.AccessLevel, 1).Span;
            var origFailsafe =
                setupData.Slice(question.BeginAddress + AmiSetupDataQuestionOffset.Failsafe, 1).Span;
            var origOptimal =
                setupData.Slice(question.BeginAddress + AmiSetupDataQuestionOffset.Optimal, 1).Span;

            var edited = false;
            if (origAccessLevel[0] != question.Question.AccessLevel)
            {
                origAccessLevel[0] = question.Question.AccessLevel;
                edited = true;
            }

            if (origFailsafe[0] != question.Question.Failsafe)
            {
                origFailsafe[0] = question.Question.Failsafe;
                edited = true;
            }

            if (origOptimal[0] != question.Question.Optimal)
            {
                origOptimal[0] = question.Question.Optimal;
                edited = true;
            }

            if (edited)
            {
                _logger.LogInformation("{type} question at SetupData offset {offset} patched",
                    question.Type, question.BeginAddress);
                patched++;
            }
        }

        _logger.LogInformation("Patched {patched} of {all} questions; skipped {errors}", patched, all, error);
    }

    public ExtractedAmiSetupDataQuestions ExtractAll(IReadOnlyList<IfrOperation> allOpCodes,
        ReadOnlyMemory<byte> setupData)
    {
        var supportedOpCodes = new[]
        {
            IfrOpCodes.Ref,
            IfrOpCodes.String,
            IfrOpCodes.Numeric,
            IfrOpCodes.CheckBox,
            IfrOpCodes.OneOf,
        };
        var opCodes = allOpCodes.Where(x => supportedOpCodes.Contains(x.Opcode)).ToList();

        var result = new ExtractedAmiSetupDataQuestions()
        {
            Questions = opCodes
                .AsParallel()
                .AsOrdered()
                .Select(x => ExtractOne(x, setupData.Span))
                .OfType<ExtractedAmiSetupDataQuestion>()
                .ToList(),
        };

        _logger.LogInformation("Found {count} questions of {ops}", result.Questions.Count, opCodes.Count);
        return result;
    }

    public ExtractedAmiSetupDataQuestion? ExtractOne(IfrOperation opCode, ReadOnlySpan<byte> setupData)
    {
        if (opCode.Fields.QuestionId == null)
        {
            _logger.LogWarning("IFR {type} opcode at offset {offset} has no question id. Skipping",
                opCode.Opcode, opCode.Offset);
            return null;
        }

        if (opCode.Fields.Help == null)
        {
            _logger.LogWarning("IFR {type} opcode at offset {offset} has no help string. Skipping",
                opCode.Opcode, opCode.Offset);
            return null;
        }

        if (opCode.Fields.Prompt == null)
        {
            _logger.LogWarning("IFR {type} opcode at offset {offset} has no prompt string. Skipping",
                opCode.Opcode, opCode.Offset);
            return null;
        }

        var pattern = new AmiSetupDataQuestionDataPattern(
            opCode.Fields.QuestionId.Value,
            opCode.Fields.Help!.Id,
            opCode.Fields.Prompt.Id
        );

        if (pattern.TryFindSingle(setupData, out var range))
        {
            var setupQuestion = pattern.Read(setupData, range);
            var l = range.GetOffsetAndLength(setupData.Length);
            return new ExtractedAmiSetupDataQuestion()
            {
                BeginAddress = l.Offset,
                EndAddress = l.Offset + l.Length,
                Type = opCode.Opcode,
                Question = setupQuestion,
            };
        }

        _logger.LogWarning("IFR {type} opcode at offset {offset} was not found in SetupData. Skipping",
            opCode.Opcode, opCode.Offset);
        return null;
    }
}
