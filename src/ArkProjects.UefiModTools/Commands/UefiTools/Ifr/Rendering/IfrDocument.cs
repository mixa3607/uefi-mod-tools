using System.Text.Json;
using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Ifr.Structures;

namespace ArkProjects.UefiModTools.Commands.UefiTools.Ifr.Rendering;

public class IfrDocument
{
    public const int SupportedVersion = 1;
    public const string SupportedType = "AMI-IFR-Render";

    public int Version { get; set; } = -1;
    public string Type { get; set; } = "Unknown";
    public required string IfrSha256 { get; set; }
    public List<IfrDocumentFormset> Formsets { get; set; } = [];
}

public class IfrDocumentFormset
{
    public string NodeType { get; set; } = string.Empty;
    public IfrDocumentSource Source { get; set; } = new();
    public string? Guid { get; set; }
    public IfrStringReference? Title { get; set; }
    public IfrStringReference? Help { get; set; }
    public List<IfrDocumentVarstore> Varstores { get; set; } = [];
    public List<IfrDocumentForm> Forms { get; set; } = [];
}

public class IfrDocumentVarstore
{
    public ushort? Id { get; set; }
    public string? Name { get; set; }
    public string? Guid { get; set; }
    public string? Kind { get; set; }
    public ushort? Size { get; set; }
    public uint? Attributes { get; set; }
}

public class IfrDocumentForm
{
    public string NodeType { get; set; } = string.Empty;
    public IfrDocumentSource Source { get; set; } = new();
    public ushort? Id { get; set; }
    public IfrStringReference? Title { get; set; }
    public List<IfrDocumentNode> Children { get; set; } = [];
}

public class IfrDocumentNode
{
    public string NodeType { get; set; } = string.Empty;
    public string Opcode { get; set; } = string.Empty;
    public IfrDocumentSource Source { get; set; } = new();
    public string? Effect { get; set; }
    public string? Kind { get; set; }
    public IfrStringReference? Prompt { get; set; }
    public IfrStringReference? Help { get; set; }
    public ushort? QuestionId { get; set; }
    public ushort? FormId { get; set; }
    public ushort? VarstoreId { get; set; }
    public ushort? VarOffset { get; set; }
    public byte? QuestionFlags { get; set; }
    public byte? Flags { get; set; }
    public IfrMinMaxStep? Range { get; set; }
    public List<IfrDocumentOption> Options { get; set; } = [];
    public List<IfrDocumentDefault> Defaults { get; set; } = [];
    public List<IfrDocumentExpression> ExpressionOperations { get; set; } = [];
    public List<IfrDocumentNode> Children { get; set; } = [];
}

public class IfrDocumentOption
{
    public IfrStringReference? Text { get; set; }
    public JsonElement? Value { get; set; }
    public bool? Default { get; set; }
    public bool? ManufacturingDefault { get; set; }
}

public class IfrDocumentDefault
{
    public ushort? Id { get; set; }
    public JsonElement? Value { get; set; }
    public IfrDocumentSource Source { get; set; } = new();
}

public class IfrDocumentExpression
{
    public string Opcode { get; set; } = string.Empty;
    public ushort? QuestionId { get; set; }
    public ushort? OtherQuestionId { get; set; }
    public ushort? ReferencedQuestionId { get; set; }
    public JsonElement? Value { get; set; }
    public IfrDocumentSource Source { get; set; } = new();
}

public class IfrDocumentSource
{
    public ulong Offset { get; set; }
    public byte Length { get; set; }
}
