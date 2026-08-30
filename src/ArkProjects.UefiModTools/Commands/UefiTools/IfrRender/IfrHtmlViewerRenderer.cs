namespace ArkProjects.UefiModTools.Commands.UefiTools.IfrRender;

public class IfrHtmlViewerRenderer
{
    public string Render(string renderedJson) => $$"""
<!doctype html>
<html lang="en">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>IFR Viewer</title>
<style>
:root { color-scheme: dark; font-family: Inter, system-ui, sans-serif; background: #111827; color: #e5e7eb; }
* { box-sizing: border-box; } body { margin: 0; }
header { height: 58px; display: flex; align-items: center; gap: 18px; padding: 0 20px; border-bottom: 1px solid #334155; background: #172033; }
h1 { margin: 0; font-size: 17px; letter-spacing: .04em; } input { width: min(480px, 55vw); padding: 9px 12px; border: 1px solid #475569; border-radius: 6px; background: #0f172a; color: inherit; }
main { display: grid; grid-template-columns: minmax(380px, 1fr) minmax(360px, .8fr); height: calc(100vh - 58px); }
#tree { overflow: auto; padding: 14px 18px 60px; border-right: 1px solid #334155; } #details { overflow: auto; padding: 20px; background: #0f172a; }
details { margin: 3px 0; } summary { cursor: pointer; padding: 5px 7px; border-radius: 5px; list-style-position: outside; } summary:hover, summary.selected { background: #26354f; }
ul { margin: 2px 0 2px 17px; padding-left: 12px; border-left: 1px solid #334155; list-style: none; }
.formset > summary { color: #93c5fd; font-weight: 700; } .form > summary { color: #c4b5fd; font-weight: 600; }
.condition > summary { color: #fbbf24; } .question > summary { color: #d1fae5; }
.badge { display: inline-block; margin-left: 7px; padding: 1px 5px; border: 1px solid #475569; border-radius: 999px; color: #94a3b8; font: 11px ui-monospace, monospace; }
.muted { color: #94a3b8; } #details h2 { margin-top: 0; font-size: 18px; } #details pre { white-space: pre-wrap; overflow-wrap: anywhere; padding: 14px; border-radius: 7px; background: #111827; border: 1px solid #334155; font: 12px/1.5 ui-monospace, monospace; }
@media (max-width: 760px) { header { height: auto; min-height: 58px; flex-wrap: wrap; padding: 10px 14px; gap: 9px; } input { width: 100%; order: 3; } main { display: block; height: auto; } #tree { min-height: 48vh; border-right: 0; border-bottom: 1px solid #334155; } #details { min-height: 45vh; } }
</style>
</head>
<body>
<header><h1>IFR Viewer</h1><input id="search" type="search" placeholder="Search prompt, QuestionId, VarOffset, condition..." autofocus><span id="count" class="muted"></span></header>
<main><section id="tree" aria-label="IFR tree"></section><aside id="details"><p class="muted">Select a question or condition to inspect its IFR and SetupData fields.</p></aside></main>
<script id="ifr-data" type="application/json">{{renderedJson}}</script>
<script>
const documentData = JSON.parse(document.getElementById('ifr-data').textContent);
const tree = document.getElementById('tree');
const details = document.getElementById('details');
const search = document.getElementById('search');
const count = document.getElementById('count');
let selected;

const text = value => value ? (value.text ?? value.Text ?? String(value)) : '';
const valueText = value => value?.value ?? value?.Value ?? JSON.stringify(value);
const expressionOperationText = operation => {
  if (operation.Opcode === 'True') return 'always true';
  if (operation.Opcode === 'False') return 'always false';
  if (operation.Opcode === 'EqIdVal') return `Question #${operation.QuestionId} equals ${valueText(operation.Value)}`;
  if (operation.Opcode === 'EqIdId') return `Question #${operation.QuestionId} equals Question #${operation.OtherQuestionId}`;
  if (operation.Opcode === 'QuestionRef1' || operation.Opcode === 'QuestionRef3') return `Question #${operation.ReferencedQuestionId}`;
  if (operation.Value != null) return `${operation.Opcode} ${valueText(operation.Value)}`;
  return operation.Opcode;
};
const expressionText = node => {
  const operations = node.ExpressionOperations ?? [];
  if (!operations.length) return 'expression unavailable';
  return operations.map(expressionOperationText).join(' -> ');
};
const affectedQuestionCount = node => (node.Children ?? []).reduce((count, child) =>
  count + (child.NodeType === 'question' ? 1 : 0) + affectedQuestionCount(child), 0);
const conditionDescription = node => `${(node.Effect ?? node.Opcode).toUpperCase()} ${affectedQuestionCount(node)} item${affectedQuestionCount(node) === 1 ? '' : 's'} when: ${expressionText(node)}`;
const nodeLabel = node => node.NodeType === 'condition'
  ? conditionDescription(node)
  : text(node.Prompt) || `${node.Opcode} #${node.QuestionId ?? '?'}`;
const nodeSearch = node => { const { Children, ...fields } = node; return JSON.stringify(fields).toLowerCase(); };
const matches = (node, filter) => !filter || nodeSearch(node).includes(filter) || (node.Children ?? []).some(child => matches(child, filter));
const formMatches = (form, filter) => !filter || JSON.stringify({ Id: form.Id, Title: form.Title }).toLowerCase().includes(filter) || (form.Children ?? []).some(node => matches(node, filter));

function select(value, element) {
  document.querySelectorAll('.selected').forEach(item => item.classList.remove('selected'));
  element.classList.add('selected');
  selected = value;
  details.replaceChildren();
  const heading = document.createElement('h2'); heading.textContent = value.NodeType ? nodeLabel(value) : text(value.Title) || 'Formset';
  const source = document.createElement('p'); source.className = 'muted'; source.textContent = value.Source ? `IFR offset ${value.Source.Offset}, length ${value.Source.Length}` : '';
  if (value.NodeType === 'condition') { const explanation = document.createElement('p'); explanation.textContent = `The nested items are affected only while this condition is true: ${expressionText(value)}.`; details.append(explanation); }
  const json = document.createElement('pre'); json.textContent = JSON.stringify(value, null, 2);
  details.append(heading, source, json);
}

function appendNode(parent, node, filter) {
  if (!matches(node, filter)) return;
  const item = document.createElement('li'); item.className = node.NodeType;
  const branch = document.createElement('details'); branch.open = Boolean(filter) || node.NodeType === 'condition';
  const summary = document.createElement('summary'); summary.textContent = nodeLabel(node);
  if (node.QuestionId != null) { const badge = document.createElement('span'); badge.className = 'badge'; badge.textContent = `QID ${node.QuestionId}`; summary.append(badge); }
  summary.addEventListener('click', () => select(node, summary));
  branch.append(summary);
  if ((node.Children ?? []).length) { const children = document.createElement('ul'); node.Children.forEach(child => appendNode(children, child, filter)); branch.append(children); }
  item.append(branch); parent.append(item);
}

function render() {
  const filter = search.value.trim().toLowerCase();
  tree.replaceChildren(); let forms = 0;
  for (const formset of documentData.Formsets ?? []) {
    const formsetBranch = document.createElement('details'); formsetBranch.open = true; formsetBranch.className = 'formset';
    const formsetSummary = document.createElement('summary'); formsetSummary.textContent = text(formset.Title) || formset.Guid || 'Formset'; formsetSummary.addEventListener('click', () => select(formset, formsetSummary)); formsetBranch.append(formsetSummary);
    const formList = document.createElement('ul');
    for (const form of formset.Forms ?? []) {
      if (!formMatches(form, filter)) continue;
      forms++; const formBranch = document.createElement('details'); formBranch.open = Boolean(filter); formBranch.className = 'form';
      const formSummary = document.createElement('summary'); formSummary.textContent = text(form.Title) || `Form ${form.Id ?? '?'}`; formSummary.addEventListener('click', () => select(form, formSummary)); formBranch.append(formSummary);
      const nodeList = document.createElement('ul'); (form.Children ?? []).forEach(node => appendNode(nodeList, node, filter)); formBranch.append(nodeList); const item = document.createElement('li'); item.append(formBranch); formList.append(item);
    }
    formsetBranch.append(formList); tree.append(formsetBranch);
  }
  count.textContent = `${forms} form${forms === 1 ? '' : 's'} shown`;
}

search.addEventListener('input', render);
render();
</script>
</body>
</html>
""";
}
