using ArkProjects.UefiModTools.Commands.UefiEditorJs.Models;
using ConsoleTables;
using Microsoft.Extensions.Logging;

namespace ArkProjects.UefiModTools.Commands.UefiEditorJs;

public class UefiEditorJsRenderCommandHandlers
{
    private readonly ILogger<UefiEditorJsRenderCommandHandlers> _logger;
    private readonly JsonSerializationService _jsonSerializer;

    public UefiEditorJsRenderCommandHandlers(ILogger<UefiEditorJsRenderCommandHandlers> logger,
        JsonSerializationService jsonSerializer)
    {
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    public int RenderMenu(string inputFile, string outputFile)
    {
        var str = CommandHelpers.ReadString(inputFile, null, _logger);
        var data = _jsonSerializer.Deserialize<Data>(str);

        _logger.LogInformation("Rendering data...");
        var tree = ProcessData(data);

        _logger.LogInformation("Writing result...");


        var table = new ConsoleTable("Path", "Type", "SuppressIf")
        {
            MaxWidth = int.MaxValue,
        };
        foreach (var section in tree)
        {
            RenderToTable(section, 0, table);
        }

        CommandHelpers.WriteResult($"```{Environment.NewLine}{table.ToMarkDownString()}```", outputFile, true, _logger);
        return 0;
    }

    private void RenderToTable(BiosSection section, int depth, ConsoleTable table)
    {
        var suppressIfStr = section.SuppressIf?.Count > 0
            ? $"[{string.Join(", ", section.SuppressIf.Select(x => (x.Active ? "🟢" : "🔘") + x.Name))}]"
            : "";

        var indent = new string(Enumerable.Repeat(' ', depth * 2).ToArray());
        var nameStr =
            $"{indent}- {(string.IsNullOrWhiteSpace(section.Name) ? "<empty>" : section.Name)}{(section.Childs?.Count > 0 ? ":" : "")}";
        table.AddRow([nameStr, section.Type, suppressIfStr,]);

        if (!(section.Childs?.Count > 0))
            return;
        foreach (var child in section.Childs)
        {
            RenderToTable(child, depth + 1, table);
        }
    }

    private List<BiosSection> ProcessData(Data data)
    {
        var biosTree = new List<BiosSection>();
        foreach (var menu in data.Menu)
        {
            var section = new BiosSection()
            {
                Name = menu.Name,
                Description = "",
                Type = "Menu",
            };
            biosTree.Add(section);
            ProcessForm(menu.FormId, data, section, 0);
        }

        return biosTree;
    }

    private void ProcessForm(string formId, Data data, BiosSection parent, int depth)
    {
        if (depth > 10)
        {
            _logger.LogWarning("Max depth reached ({depth}) in formId {id}", depth, formId);
            return;
        }

        var form = data.Forms.First(x => x.FormId == formId);
        foreach (var child in form.Children)
        {
            var childFormId = child is { FormId: not null, Type: "Ref" } ? child.FormId : null;
            if (formId == childFormId)
            {
                _logger.LogWarning("Self reference detected in formId {id}", formId);
                return;
            }

            var section = new BiosSection()
            {
                Type = child.Type,
                Name = child.Name,
                Description = child.Description,
                SuppressIf = child.SuppressIf?
                    .Select(x => new BiosSectionSuppressIf()
                    {
                        Name = x,
                        Active = data.Suppressions.First(y => y.Offset == x).Active
                    })
                    .ToList(),
            };
            parent.Childs ??= [];
            parent.Childs.Add(section);

            if (childFormId != null)
            {
                ProcessForm(childFormId, data, section, depth + 1);
            }
        }
    }
}

public class BiosSection
{
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Description { get; set; }
    public List<BiosSection>? Childs { get; set; }
    public List<BiosSectionSuppressIf>? SuppressIf { get; set; }
}

public class BiosSectionSuppressIf
{
    public required string Name { get; set; }
    public required bool Active { get; set; }
}
