using System.Text.Json;
using ArkProjects.UefiModTools.Ifr.Structures;
using ArkProjects.UefiModTools.Commands.UefiTools.IfrSetupData;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

public class IfrTreeRenderer
{
    private static readonly HashSet<string> QuestionOpcodes =
    [
        "Action", "CheckBox", "Date", "Numeric", "OneOf", "OrderedList", "Password", "Ref", "ResetButton", "String", "Time",
    ];

    private static readonly Dictionary<string, string> ConditionEffects = new()
    {
        ["SuppressIf"] = "suppress",
        ["DisableIf"] = "disable",
        ["GrayOutIf"] = "grayout",
        ["NoSubmitIf"] = "no_submit",
        ["InconsistentIf"] = "inconsistent",
        ["WarningIf"] = "warning",
    };

    public IfrRenderDocument Render(IReadOnlyList<IfrOperation> operations,
        IReadOnlyList<ExtractedAmiSetupDataQuestion>? setupDataQuestions = null)
    {
        var root = BuildScopeTree(operations);
        var setupDataQuestionsByKey = (setupDataQuestions ?? [])
            .GroupBy(x => (x.Type, x.Question.QuestionId, x.Question.HelpStringId, x.Question.PromptStringId))
            .ToDictionary(x => x.Key, x => x.First());
        return new IfrRenderDocument
        {
            Formsets = root.Children
                .Where(x => x.Operation.Opcode == "FormSet")
                .Select(x => BuildFormset(x, setupDataQuestionsByKey))
                .ToList(),
        };
    }

    private static IfrRenderFormset BuildFormset(ScopeNode formset,
        IReadOnlyDictionary<(string Type, ushort QuestionId, ushort HelpStringId, ushort PromptStringId), ExtractedAmiSetupDataQuestion> setupDataQuestions)
    {
        return new IfrRenderFormset
        {
            Guid = formset.Operation.Fields.Guid,
            Title = formset.Operation.Fields.Title,
            Help = formset.Operation.Fields.Help,
            Varstores = formset.Descendants()
                .Where(x => x.Operation.Opcode is "VarStore" or "VarStoreEfi" or "VarStoreNameValue")
                .Select(x => new IfrRenderVarstore
                {
                    Id = x.Operation.Fields.VarStoreId,
                    Name = GetString(x.Operation.Fields.Name),
                    Guid = x.Operation.Fields.Guid,
                    Kind = x.Operation.Fields.Kind,
                    Size = x.Operation.Fields.Size,
                    Attributes = x.Operation.Fields.Attributes,
                })
                .ToList(),
            Forms = formset.Descendants()
                .Where(x => x.Operation.Opcode == "Form")
                .Select(x => new IfrRenderForm
                {
                    Id = x.Operation.Fields.FormId,
                    Title = x.Operation.Fields.Title,
                    Children = RenderNodes(x.Children, setupDataQuestions),
                })
                .ToList(),
        };
    }

    private static List<IfrRenderNode> RenderNodes(IEnumerable<ScopeNode> nodes,
        IReadOnlyDictionary<(string Type, ushort QuestionId, ushort HelpStringId, ushort PromptStringId), ExtractedAmiSetupDataQuestion> setupDataQuestions)
    {
        return nodes
            .Where(x => x.Operation.Opcode != "End")
            .Select(x => RenderNode(x, setupDataQuestions))
            .OfType<IfrRenderNode>()
            .ToList();
    }

    private static IfrRenderNode? RenderNode(ScopeNode node,
        IReadOnlyDictionary<(string Type, ushort QuestionId, ushort HelpStringId, ushort PromptStringId), ExtractedAmiSetupDataQuestion> setupDataQuestions)
    {
        var operation = node.Operation;
        if (ConditionEffects.TryGetValue(operation.Opcode, out var effect))
        {
            return new IfrRenderNode
            {
                NodeType = "condition",
                Opcode = operation.Opcode,
                Effect = effect,
                Source = CreateSource(operation),
                ExpressionOperations = node.Children
                    .Where(x => !QuestionOpcodes.Contains(x.Operation.Opcode) && !ConditionEffects.ContainsKey(x.Operation.Opcode))
                    .Select(CreateExpression)
                    .ToList(),
                Children = RenderNodes(node.Children, setupDataQuestions),
            };
        }

        if (!QuestionOpcodes.Contains(operation.Opcode))
        {
            return null;
        }

        return new IfrRenderNode
        {
            NodeType = "question",
            Opcode = operation.Opcode,
            Source = CreateSource(operation),
            Kind = operation.Fields.Kind,
            Prompt = operation.Fields.Prompt,
            Help = operation.Fields.Help,
            QuestionId = operation.Fields.QuestionId,
            VarstoreId = operation.Fields.VarStoreId,
            VarOffset = operation.Fields.VarOffset,
            QuestionFlags = operation.Fields.QuestionFlags,
            Flags = operation.Fields.Flags,
            Range = operation.Fields.MinMaxStep,
            SetupDataQuestion = FindSetupDataQuestion(operation, setupDataQuestions),
            Options = node.Descendants()
                .Where(x => x.Operation.Opcode == "OneOfOption")
                .Select(x => new IfrRenderOption
                {
                    Text = x.Operation.Fields.Option,
                    Value = x.Operation.Fields.Value,
                    Default = x.Operation.Fields.Default,
                    ManufacturingDefault = x.Operation.Fields.MfgDefault,
                })
                .ToList(),
            Defaults = node.Descendants()
                .Where(x => x.Operation.Opcode == "Default")
                .Select(x => new IfrRenderDefault
                {
                    Id = x.Operation.Fields.DefaultId,
                    Value = x.Operation.Fields.Value,
                })
                .ToList(),
            Children = RenderNodes(node.Children, setupDataQuestions),
        };
    }

    private static IfrRenderSetupDataQuestion? FindSetupDataQuestion(IfrOperation operation,
        IReadOnlyDictionary<(string Type, ushort QuestionId, ushort HelpStringId, ushort PromptStringId), ExtractedAmiSetupDataQuestion> setupDataQuestions)
    {
        if (operation.Fields.QuestionId is not { } questionId || operation.Fields.Help is not { } help ||
            operation.Fields.Prompt is not { } prompt ||
            !setupDataQuestions.TryGetValue((operation.Opcode, questionId, help.Id, prompt.Id), out var setupDataQuestion))
        {
            return null;
        }

        return new IfrRenderSetupDataQuestion
        {
            BeginAddress = setupDataQuestion.BeginAddress,
            EndAddress = setupDataQuestion.EndAddress,
            QuestionId = setupDataQuestion.Question.QuestionId,
            PageId = setupDataQuestion.Question.PageId,
            AccessLevel = setupDataQuestion.Question.AccessLevel,
            HelpStringId = setupDataQuestion.Question.HelpStringId,
            PromptStringId = setupDataQuestion.Question.PromptStringId,
            Failsafe = setupDataQuestion.Question.Failsafe,
            Optimal = setupDataQuestion.Question.Optimal,
        };
    }

    private static ScopeNode BuildScopeTree(IReadOnlyList<IfrOperation> operations)
    {
        var root = new ScopeNode(new IfrOperation { Opcode = "Root" });
        var stack = new Stack<ScopeNode>();
        stack.Push(root);

        foreach (var operation in operations)
        {
            if (operation.Opcode == "End")
            {
                if (stack.Count > 1)
                {
                    stack.Pop();
                }

                continue;
            }

            var node = new ScopeNode(operation);
            stack.Peek().Children.Add(node);
            if (operation.ScopeStart)
            {
                stack.Push(node);
            }
        }

        return root;
    }

    private static IfrRenderExpression CreateExpression(ScopeNode node) => new()
    {
        Opcode = node.Operation.Opcode,
        QuestionId = node.Operation.Fields.QuestionId,
        OtherQuestionId = node.Operation.Fields.OtherQuestionId,
        ReferencedQuestionId = node.Operation.Fields.RefQuestionId,
        Value = node.Operation.Fields.Value,
        Source = CreateSource(node.Operation),
    };

    private static IfrRenderSource CreateSource(IfrOperation operation) => new()
    {
        Offset = operation.Offset,
        Length = operation.Length,
    };

    private static string? GetString(JsonElement? value) => value switch
    {
        { ValueKind: JsonValueKind.String } => value.Value.GetString(),
        { } => value.Value.GetRawText(),
        _ => null,
    };

    private sealed class ScopeNode
    {
        public ScopeNode(IfrOperation operation)
        {
            Operation = operation;
        }

        public IfrOperation Operation { get; }
        public List<ScopeNode> Children { get; } = [];

        public IEnumerable<ScopeNode> Descendants()
        {
            foreach (var child in Children)
            {
                yield return child;
                foreach (var descendant in child.Descendants())
                {
                    yield return descendant;
                }
            }
        }
    }
}
