using ArkProjects.UefiModTools.Utils;
using ArkProjects.UefiModTools.Utils.BinDataPattern;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr;

public sealed class AmiSetupDataQuestionDataPattern : IBinaryDataPattern<AmiSetupDataQuestion>
{
    private readonly BinaryDataPatternByte[] _mask = new BinaryDataPatternByte[AmiSetupDataQuestion.Size];

    public AmiSetupDataQuestionDataPattern(ushort questionId, ushort helpId, ushort promptId)
    {
        Array.Fill(_mask, BinaryDataPatternByte.Any);
        SetUInt16(AmiSetupDataQuestionOffset.QuestionId, questionId);
        SetUInt16(AmiSetupDataQuestionOffset.HelpStringId, helpId);
        SetUInt16(AmiSetupDataQuestionOffset.PromptStringId, promptId);
    }

    private void SetUInt16(int offset, ushort value)
    {
        _mask[offset + 0] = new BinaryDataPatternByte((byte)(value >> 0));
        _mask[offset + 1] = new BinaryDataPatternByte((byte)(value >> 8));
    }

    public bool IsMatch(ReadOnlySpan<byte> setupData)
    {
        if (setupData.Length < AmiSetupDataQuestion.Size)
        {
            return false;
        }

        for (int i = 0; i < _mask.Length; i++)
        {
            if (!_mask[i].IsMatch(setupData[i]))
            {
                return false;
            }
        }

        return true;
    }

    public bool TryFindSingle(ReadOnlySpan<byte> setupData, out Range match)
    {
        match = new Range(0, 0);
        for (int i = 0; i <= setupData.Length - _mask.Length; i++)
        {
            var range = setupData.Slice(i, _mask.Length);
            if (!IsMatch(range))
                continue;

            if (match.IsEmpty(setupData.Length))
                match = new Range(i, i + _mask.Length);
            else
                return false;
        }

        return match.End.Value > 0;
    }

    public AmiSetupDataQuestion Read(ReadOnlySpan<byte> bytes, Range range)
    {
        return MarshalHelper.FromBytes<AmiSetupDataQuestion>(bytes.Slice(range));
    }
}
