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

    public List<ExtractedAmiSetupDataQuestion> ExtractAll(IReadOnlyList<IfrOperation> allOpCodes, ReadOnlyMemory<byte> setupData)
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

        var questions = opCodes
            .AsParallel()
            .AsOrdered()
            .Select(x => ExtractOne(x, setupData.Span))
            .OfType<ExtractedAmiSetupDataQuestion>()
            .ToList();
        var duplicateId = questions.GroupBy(x => x.Id).FirstOrDefault(x => x.Count() > 1);
        if (duplicateId != null)
        {
            throw new InvalidDataException($"SetupData map contains duplicate question id '{duplicateId.Key}'");
        }

        _logger.LogInformation("Found {count} questions of {ops}", questions.Count, opCodes.Count);
        return questions;
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
                Id = $"{opCode.Opcode}-{opCode.Fields.QuestionId.Value:X4}",
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
