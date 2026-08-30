using ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;
using ArkProjects.UefiModTools.Ifr.Structures;
using Xunit;

namespace ArkProjects.UefiModTools.Tests.Commands.UefiTools.IfrRender;

public class IfrTreeRendererTests
{
    [Fact]
    public void RenderBuildsFormQuestionAndConditionTree()
    {
        var document = new IfrTreeRenderer().Render(CreateOperations(),
        [
            new ExtractedAmiSetupDataQuestion
            {
                BeginAddress = 100,
                EndAddress = 154,
                Type = "OneOf",
                Question = new AmiSetupDataQuestion
                {
                    QuestionId = 18,
                    PageId = 2,
                    AccessLevel = 3,
                    HelpStringId = 1243,
                    PromptStringId = 1242,
                    Failsafe = 0,
                    Optimal = 1,
                },
            },
        ]);

        var formset = Assert.Single(document.Formsets);
        Assert.Equal("formset", formset.NodeType);
        Assert.Equal((ulong)10, formset.Source.Offset);
        Assert.Equal("Platform Configuration", formset.Title!.Text);
        var varstore = Assert.Single(formset.Varstores);
        Assert.Equal((ushort)1, varstore.Id);
        Assert.Equal("IntelSetup", varstore.Name);

        var form = Assert.Single(formset.Forms);
        Assert.Equal("form", form.NodeType);
        Assert.Equal((ulong)30, form.Source.Offset);
        var condition = Assert.Single(form.Children);
        Assert.Equal("condition", condition.NodeType);
        Assert.Equal("grayout", condition.Effect);
        Assert.Equal("True", Assert.Single(condition.ExpressionOperations).Opcode);

        var question = Assert.Single(condition.Children);
        Assert.Equal("question", question.NodeType);
        Assert.Equal("OneOf", question.Opcode);
        Assert.Equal("PFR Supported", question.Prompt!.Text);
        Assert.Equal((ushort)18, question.QuestionId);
        Assert.Equal((ushort)1, question.VarstoreId);
        Assert.Equal((ushort)241, question.VarOffset);
        Assert.Equal(100, question.SetupDataQuestion!.BeginAddress);
        Assert.Equal((byte)3, question.SetupDataQuestion.AccessLevel);
        Assert.Equal((byte)1, question.SetupDataQuestion.Optimal);
        Assert.Equal("Yes", Assert.Single(question.Options).Text!.Text);
        Assert.Equal((ushort)0, Assert.Single(question.Defaults).Id);
    }

    private static List<IfrOperation> CreateOperations() =>
    [
        Operation("FormSet", true, 10, fields =>
        {
            fields.Guid = "formset-guid";
            fields.Title = Text(1155, "Platform Configuration");
        }),
        Operation("VarStoreEfi", false, 20, fields =>
        {
            fields.VarStoreId = 1;
            fields.Name = System.Text.Json.JsonDocument.Parse("\"IntelSetup\"").RootElement.Clone();
            fields.Kind = "efi";
            fields.Guid = "formset-guid";
            fields.Size = 538;
        }),
        Operation("Form", true, 30, fields =>
        {
            fields.FormId = 1;
            fields.Title = Text(1155, "Platform Configuration");
        }),
        Operation("GrayOutIf", true, 40),
        Operation("True", false, 41),
        Operation("OneOf", true, 50, fields =>
        {
            fields.Kind = "oneof";
            fields.Prompt = Text(1242, "PFR Supported");
            fields.Help = Text(1243, "Whether the platform supports PFR.");
            fields.QuestionId = 18;
            fields.VarStoreId = 1;
            fields.VarOffset = 241;
        }),
        Operation("OneOfOption", false, 51, fields =>
        {
            fields.Option = Text(1, "Yes");
        }),
        Operation("Default", false, 52, fields => fields.DefaultId = 0),
        Operation("End", false, 53),
        Operation("End", false, 54),
        Operation("End", false, 55),
        Operation("End", false, 56),
    ];

    private static IfrOperation Operation(string opcode, bool scopeStart, ulong offset,
        Action<IfrOperationFields>? configure = null)
    {
        var fields = new IfrOperationFields();
        configure?.Invoke(fields);
        return new IfrOperation { Opcode = opcode, ScopeStart = scopeStart, Offset = offset, Fields = fields };
    }

    private static IfrStringReference Text(ushort id, string text) => new() { Id = id, Text = text };
}
