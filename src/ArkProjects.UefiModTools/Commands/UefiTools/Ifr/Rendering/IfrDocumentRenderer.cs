using System.Text.Json;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;

public class IfrDocumentRenderer
{
    private static readonly HashSet<string> QuestionOpcodes =
    [
        IfrOpCodes.Action, IfrOpCodes.CheckBox, IfrOpCodes.Date, IfrOpCodes.Numeric, IfrOpCodes.OneOf,
        IfrOpCodes.OrderedList, IfrOpCodes.Password, IfrOpCodes.Ref, IfrOpCodes.ResetButton, IfrOpCodes.String,
        IfrOpCodes.Time,
    ];

    private static readonly Dictionary<string, string> ConditionEffects = new()
    {
        [IfrOpCodes.SuppressIf] = "suppress",
        [IfrOpCodes.DisableIf] = "disable",
        [IfrOpCodes.GrayOutIf] = "grayout",
        [IfrOpCodes.NoSubmitIf] = "no_submit",
        [IfrOpCodes.InconsistentIf] = "inconsistent",
        [IfrOpCodes.WarningIf] = "warning",
    };

    public List<IfrDocumentFormset> RenderFormsets(IReadOnlyList<IfrOperation> operations)
    {
        var root = BuildScopeTree(operations);
        return root.Children
                .Where(x => x.Operation.Opcode == IfrOpCodes.FormSet)
                .Select(BuildFormset)
                .ToList();
    }

    private static IfrDocumentFormset BuildFormset(ScopeNode formset)
    {
        return new IfrDocumentFormset
        {
            NodeType = "formset",
            Source = CreateSource(formset.Operation),
            Guid = formset.Operation.Fields.Guid,
            Title = formset.Operation.Fields.Title,
            Help = formset.Operation.Fields.Help,
            Varstores = formset.Descendants()
                .Where(x => x.Operation.Opcode is IfrOpCodes.VarStore or IfrOpCodes.VarStoreEfi or IfrOpCodes.VarStoreNameValue)
                .Select(x => new IfrDocumentVarstore
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
                .Where(x => x.Operation.Opcode == IfrOpCodes.Form)
                .Select(x => new IfrDocumentForm
                {
                    NodeType = "form",
                    Source = CreateSource(x.Operation),
                    Id = x.Operation.Fields.FormId,
                    Title = x.Operation.Fields.Title,
                    Children = RenderNodes(x.Children),
                })
                .ToList(),
        };
    }

    private static List<IfrDocumentNode> RenderNodes(IEnumerable<ScopeNode> nodes)
    {
        return nodes
            .Where(x => x.Operation.Opcode != IfrOpCodes.End)
            .Select(RenderNode)
            .OfType<IfrDocumentNode>()
            .ToList();
    }

    private static IfrDocumentNode? RenderNode(ScopeNode node)
    {
        var operation = node.Operation;
        if (ConditionEffects.TryGetValue(operation.Opcode, out var effect))
        {
            return new IfrDocumentNode
            {
                NodeType = "condition",
                Opcode = operation.Opcode,
                Effect = effect,
                Source = CreateSource(operation),
                ExpressionOperations = node.Children
                    .Where(x => !QuestionOpcodes.Contains(x.Operation.Opcode) && !ConditionEffects.ContainsKey(x.Operation.Opcode))
                    .Select(CreateExpression)
                    .ToList(),
                Children = RenderNodes(node.Children),
            };
        }

        if (!QuestionOpcodes.Contains(operation.Opcode))
        {
            return null;
        }

        return new IfrDocumentNode
        {
            NodeType = "question",
            Opcode = operation.Opcode,
            Source = CreateSource(operation),
            Kind = operation.Fields.Kind,
            Prompt = operation.Fields.Prompt,
            Help = operation.Fields.Help,
            QuestionId = operation.Fields.QuestionId,
            FormId = operation.Fields.FormId,
            VarstoreId = operation.Fields.VarStoreId,
            VarOffset = operation.Fields.VarOffset,
            QuestionFlags = operation.Fields.QuestionFlags,
            Flags = operation.Fields.Flags,
            Range = operation.Fields.MinMaxStep,
            Options = node.Descendants()
                .Where(x => x.Operation.Opcode == IfrOpCodes.OneOfOption)
                .Select(x => new IfrDocumentOption
                {
                    Text = x.Operation.Fields.Option,
                    Value = x.Operation.Fields.Value,
                    Default = x.Operation.Fields.Default,
                    ManufacturingDefault = x.Operation.Fields.MfgDefault,
                })
                .ToList(),
            Defaults = node.Descendants()
                .Where(x => x.Operation.Opcode == IfrOpCodes.Default)
                .Select(x => new IfrDocumentDefault
                {
                    Id = x.Operation.Fields.DefaultId,
                    Value = x.Operation.Fields.Value,
                })
                .ToList(),
            Children = RenderNodes(node.Children),
        };
    }


    private static ScopeNode BuildScopeTree(IReadOnlyList<IfrOperation> operations)
    {
        var root = new ScopeNode(new IfrOperation { Opcode = "Root" });
        var stack = new Stack<ScopeNode>();
        stack.Push(root);

        foreach (var operation in operations)
        {
            if (operation.Opcode == IfrOpCodes.End)
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

    private static IfrDocumentExpression CreateExpression(ScopeNode node) => new()
    {
        Opcode = node.Operation.Opcode,
        QuestionId = node.Operation.Fields.QuestionId,
        OtherQuestionId = node.Operation.Fields.OtherQuestionId,
        ReferencedQuestionId = node.Operation.Fields.RefQuestionId,
        Value = node.Operation.Fields.Value,
        Source = CreateSource(node.Operation),
    };

    private static IfrDocumentSource CreateSource(IfrOperation operation) => new()
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
