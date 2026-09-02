using System.Text.Json;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;
using ArkProjects.UefiModTools.Ifr.Structures;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrSetupData;

public class SetupDataParserTests
{
    [Fact]
    public void ExtractAllReadsQuestionFromRealSetupDataSlice()
    {
        var result = CreateParser().ExtractAll([CreateOperation()], ReadFixture());

        var question = Assert.Single(result);
        Assert.Equal(0, question.BeginAddress);
        Assert.Equal(AmiSetupDataQuestion.Size, question.EndAddress);
        Assert.Equal("OneOf-0001", question.Id);
        Assert.Equal("OneOf", question.Type);
        Assert.Equal((ushort)1, question.Question.QuestionId);
        Assert.Equal((ushort)1182, question.Question.HelpStringId);
        Assert.Equal((ushort)1181, question.Question.PromptStringId);
        Assert.Equal((byte)9, question.Question.AccessLevel);
    }

    [Fact]
    public void PatchApplierUpdatesOnlySpecifiedQuestionValues()
    {
        var parser = CreateParser();
        var setupData = ReadFixture();
        var map = parser.ExtractAll([CreateOperation()], setupData);
        var question = Assert.Single(map);
        var originalOptimal = question.Question.Optimal;

        new SetupDataPatchApplier(NullLogger<SetupDataPatchApplier>.Instance).Apply(setupData, map,
        [
            new SetupDataQuestionPatch
            {
                Id = question.Id,
                AccessLevel = 0,
                Failsafe = 1,
            },
        ]);

        var patched = Assert.Single(parser.ExtractAll([CreateOperation()], setupData));
        Assert.Equal((byte)0, patched.Question.AccessLevel);
        Assert.Equal((byte)1, patched.Question.Failsafe);
        Assert.Equal(originalOptimal, patched.Question.Optimal);
    }

    [Fact]
    public void SerializesQuestionFieldsForEditablePatchJson()
    {
        var question = Assert.Single(CreateParser().ExtractAll([CreateOperation()], ReadFixture()));

        var json = JsonSerializer.Serialize(question);

        Assert.Contains("\"questionId\":1", json);
        Assert.Contains("\"accessLevel\":9", json);
        Assert.Contains("\"helpStringId\":1182", json);
    }

    private static SetupDataParser CreateParser() => new(NullLogger<SetupDataParser>.Instance);

    private static byte[] ReadFixture() => File.ReadAllBytes("test-files/ifr/SetupData_question_0001.bin");

    private static IfrOperation CreateOperation() => new()
    {
        Opcode = "OneOf",
        Fields = new IfrOperationFields
        {
            QuestionId = 1,
            Help = new IfrStringReference { Id = 1182 },
            Prompt = new IfrStringReference { Id = 1181 },
        },
    };
}
