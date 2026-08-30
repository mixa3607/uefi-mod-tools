using System.Text.Json;
using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

public class IfrRenderDocument
{
    public string Schema { get; set; } = "ifr-render-tree/v1";
    public List<IfrRenderFormset> Formsets { get; set; } = [];
}

public class IfrRenderFormset
{
    public string? Guid { get; set; }
    public IfrStringReference? Title { get; set; }
    public IfrStringReference? Help { get; set; }
    public List<IfrRenderVarstore> Varstores { get; set; } = [];
    public List<IfrRenderForm> Forms { get; set; } = [];
}

public class IfrRenderVarstore
{
    public ushort? Id { get; set; }
    public string? Name { get; set; }
    public string? Guid { get; set; }
    public string? Kind { get; set; }
    public ushort? Size { get; set; }
    public uint? Attributes { get; set; }
}

public class IfrRenderForm
{
    public ushort? Id { get; set; }
    public IfrStringReference? Title { get; set; }
    public List<IfrRenderNode> Children { get; set; } = [];
}

public class IfrRenderNode
{
    public string NodeType { get; set; } = string.Empty;
    public string Opcode { get; set; } = string.Empty;
    public IfrRenderSource Source { get; set; } = new();
    public string? Effect { get; set; }
    public string? Kind { get; set; }
    public IfrStringReference? Prompt { get; set; }
    public IfrStringReference? Help { get; set; }
    public ushort? QuestionId { get; set; }
    public ushort? VarstoreId { get; set; }
    public ushort? VarOffset { get; set; }
    public byte? QuestionFlags { get; set; }
    public byte? Flags { get; set; }
    public IfrMinMaxStep? Range { get; set; }
    public IfrRenderSetupDataQuestion? SetupDataQuestion { get; set; }
    public List<IfrRenderOption> Options { get; set; } = [];
    public List<IfrRenderDefault> Defaults { get; set; } = [];
    public List<IfrRenderExpression> ExpressionOperations { get; set; } = [];
    public List<IfrRenderNode> Children { get; set; } = [];
}

public class IfrRenderSetupDataQuestion
{
    public int BeginAddress { get; set; }
    public int EndAddress { get; set; }
    public ushort QuestionId { get; set; }
    public ushort PageId { get; set; }
    public byte AccessLevel { get; set; }
    public ushort HelpStringId { get; set; }
    public ushort PromptStringId { get; set; }
    public byte Failsafe { get; set; }
    public byte Optimal { get; set; }
}

public class IfrRenderOption
{
    public IfrStringReference? Text { get; set; }
    public JsonElement? Value { get; set; }
    public bool? Default { get; set; }
    public bool? ManufacturingDefault { get; set; }
}

public class IfrRenderDefault
{
    public ushort? Id { get; set; }
    public JsonElement? Value { get; set; }
}

public class IfrRenderExpression
{
    public string Opcode { get; set; } = string.Empty;
    public ushort? QuestionId { get; set; }
    public ushort? OtherQuestionId { get; set; }
    public ushort? ReferencedQuestionId { get; set; }
    public JsonElement? Value { get; set; }
    public IfrRenderSource Source { get; set; } = new();
}

public class IfrRenderSource
{
    public ulong Offset { get; set; }
    public byte Length { get; set; }
}
