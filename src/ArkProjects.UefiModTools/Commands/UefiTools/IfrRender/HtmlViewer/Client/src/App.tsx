import { useEffect, useMemo, useState } from 'react';
import { Alert, AppBar, Box, Button, Chip, CssBaseline, Divider, Drawer, FormControl, IconButton, InputLabel, MenuItem, Paper, Popover, Select, Stack, Switch, TextField, ThemeProvider, Toolbar, Tooltip, Typography, createTheme } from '@mui/material';
import { AccountTree, Brightness4, Brightness7, Download, FolderOpen, InsertDriveFile, Settings, Upload } from '@mui/icons-material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view';
import type { Document, Expression, Node, NodeRef, Option, SetupPatchQuestion } from './types';

const label = (value?: { text: string }) => value?.text ?? '';
const nodeId = (node: Node) => `${node.NodeType}:${node.Source.Offset}`;
const optionValue = (option: Option) => typeof option.Value === 'number' ? option.Value : option.Value?.value;
const expressionText = (expression: Expression) => expression.Opcode === 'True' ? 'always true' : expression.Opcode === 'False' ? 'always false' : expression.Opcode === 'EqIdVal' ? `Question #${expression.QuestionId} equals ${String(expression.Value)}` : expression.Opcode === 'EqIdId' ? `Question #${expression.QuestionId} equals Question #${expression.OtherQuestionId}` : expression.Opcode;
const conditionText = (node: Node) => `${(node.Effect ?? node.Opcode).toUpperCase()} when ${node.ExpressionOperations.map(expressionText).join(' -> ') || 'expression unavailable'}`;

function indexDocument(document: Document) {
  const byId = new Map<string, NodeRef>();
  const questionsById = new Map<number, NodeRef[]>();
  document.Formsets.forEach((formset, formsetIndex) => formset.Forms.forEach((form, formIndex) => {
    const formId = `form:${formsetIndex}:${formIndex}`;
    const visit = (node: Node, parentIds: string[]) => {
      const reference = { id: nodeId(node), node, parentIds, formTitle: label(form.Title) || `Form ${form.Id ?? '?'}` };
      byId.set(reference.id, reference);
      if (node.QuestionId != null) questionsById.set(node.QuestionId, [...(questionsById.get(node.QuestionId) ?? []), reference]);
      node.Children.forEach(child => visit(child, [...parentIds, reference.id]));
    };
    form.Children.forEach(node => visit(node, [`formset:${formsetIndex}`, formId]));
  }));
  return { byId, questionsById };
}

function download(name: string, value: unknown) {
  const url = URL.createObjectURL(new Blob([JSON.stringify(value, null, 2)], { type: 'application/json' }));
  const link = document.createElement('a'); link.href = url; link.download = name; link.click(); setTimeout(() => URL.revokeObjectURL(url), 0);
}

function setupPatch(node: Node, patches: Record<number, SetupPatchQuestion>) {
  const source = node.SetupDataQuestion!;
  return patches[source.BeginAddress] ?? { beginAddress: source.BeginAddress, endAddress: source.EndAddress, type: node.Opcode, question: { questionId: source.QuestionId, pageId: source.PageId, accessLevel: source.AccessLevel, helpStringId: source.HelpStringId, promptStringId: source.PromptStringId, failsafe: source.Failsafe, optimal: source.Optimal } };
}

function QuestionReference({ questionId, index, navigate }: { questionId: number; index: ReturnType<typeof indexDocument>; navigate: (reference: NodeRef) => void }) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const targets = index.questionsById.get(questionId) ?? [];
  return <><Button size="small" onMouseEnter={event => setAnchor(event.currentTarget)} onClick={() => targets[0] && navigate(targets[0])}>Question #{questionId}</Button><Popover open={Boolean(anchor)} anchorEl={anchor} onClose={() => setAnchor(null)} disableRestoreFocus anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}><Box sx={{ p: 1, maxWidth: 360 }}>{targets.length ? targets.map(target => <Button key={target.id} fullWidth sx={{ justifyContent: 'flex-start', textTransform: 'none' }} onClick={() => { navigate(target); setAnchor(null); }}><Stack alignItems="flex-start"><Typography>{label(target.node.Prompt) || target.node.Opcode}</Typography><Typography variant="caption" color="text.secondary">{target.formTitle}, IFR 0x{target.node.Source.Offset.toString(16).toUpperCase()}</Typography></Stack></Button>) : <Typography variant="body2">Question is not present in this document.</Typography>}</Box></Popover></>;
}

export function App({ document: viewerDocument }: { document: Document }) {
  const index = useMemo(() => indexDocument(viewerDocument), [viewerDocument]);
  const [selectedId, setSelectedId] = useState<string>();
  const [expanded, setExpanded] = useState<string[]>([]);
  const [query, setQuery] = useState('');
  const [themeMode, setThemeMode] = useState<'dark' | 'light'>('dark');
  const [rawOpen, setRawOpen] = useState(false);
  const [setupPatches, setSetupPatches] = useState<Record<number, SetupPatchQuestion>>({});
  const [disabledSuppressions, setDisabledSuppressions] = useState<number[]>([]);
  const theme = useMemo(() => createTheme({ palette: { mode: themeMode, primary: { main: themeMode === 'dark' ? '#75b5ff' : '#005fb8' } }, typography: { fontSize: 13, fontFamily: 'Segoe UI, system-ui, sans-serif' }, shape: { borderRadius: 3 }, components: { MuiPaper: { styleOverrides: { root: { backgroundImage: 'none' } } } } }), [themeMode]);
  const selected = selectedId ? index.byId.get(selectedId) : undefined;
  useEffect(() => { if (!selectedId) setSelectedId(index.byId.keys().next().value); }, [index, selectedId]);
  const navigate = (reference: NodeRef) => { setExpanded(current => [...new Set([...current, ...reference.parentIds])]); setSelectedId(reference.id); setTimeout(() => document.getElementById(`tree-${reference.id}`)?.scrollIntoView({ block: 'center' }), 0); };
  const patchSetup = (node: Node, property: keyof SetupPatchQuestion['question'], value: number) => { const patch = setupPatch(node, setupPatches); setSetupPatches(current => ({ ...current, [patch.beginAddress]: { ...patch, question: { ...patch.question, [property]: value } } })); };
  const matches = (node: Node): boolean => !query || JSON.stringify(node).toLowerCase().includes(query.toLowerCase()) || node.Children.some(matches);
  const renderNode = (node: Node) => {
    if (!matches(node)) return null;
    const id = nodeId(node); const isCondition = node.NodeType === 'condition';
    const title = isCondition ? conditionText(node) : label(node.Prompt) || `${node.Opcode} #${node.QuestionId ?? '?'}`;
    return <TreeItem key={id} itemId={id} id={`tree-${id}`} label={<Stack direction="row" spacing={0.75} alignItems="center" minWidth={0}><Chip size="small" color={isCondition ? 'warning' : 'default'} label={isCondition ? (node.Effect ?? node.Opcode) : `QID ${node.QuestionId ?? '?'}`} /><Tooltip title={title}><Typography variant="body2" noWrap>{title}</Typography></Tooltip></Stack>}>{node.Children.map(renderNode)}</TreeItem>;
  };
  const setImport = async (file: File | undefined, kind: 'setup' | 'sct') => { if (!file) return; try { const patch = JSON.parse(await file.text()); if (kind === 'setup' && Array.isArray(patch.questions)) setSetupPatches(Object.fromEntries(patch.questions.map((question: SetupPatchQuestion) => [question.beginAddress, question]))); else if (kind === 'sct' && Array.isArray(patch.suppressIfPatches)) setDisabledSuppressions(patch.suppressIfPatches.filter((item: { disable: boolean }) => item.disable).map((item: { offset: number }) => item.offset)); else throw new Error(); } catch { alert('The selected file is not a compatible patch.'); } };
  const defaults = (node: Node) => node.Options.map(option => ({ value: optionValue(option), text: label(option.Text) })).filter(option => Number.isInteger(option.value) && option.value! >= 0 && option.value! <= 255) as { value: number; text: string }[];
  const inspector = !selected ? <Typography color="text.secondary">Select a question or condition.</Typography> : selected.node.NodeType === 'question' ? <QuestionInspector node={selected.node} patch={selected.node.SetupDataQuestion ? setupPatch(selected.node, setupPatches) : undefined} defaults={defaults(selected.node)} onPatch={patchSetup} /> : <ConditionInspector node={selected.node} index={index} navigate={navigate} disabled={disabledSuppressions.includes(selected.node.Source.Offset)} onDisabled={checked => setDisabledSuppressions(current => checked ? [...new Set([...current, selected.node.Source.Offset])] : current.filter(offset => offset !== selected.node.Source.Offset))} />;
  return <ThemeProvider theme={theme}><CssBaseline /><Box sx={{ height: '100vh', display: 'grid', gridTemplateRows: '48px minmax(0, 1fr)' }}><AppBar position="static" color="default" elevation={0}><Toolbar variant="dense" sx={{ gap: 1 }}><AccountTree color="primary" /><Typography variant="subtitle2" sx={{ mr: 1 }}>IFR Viewer</Typography><TextField size="small" placeholder="Search IFR" value={query} onChange={event => setQuery(event.target.value)} sx={{ width: 280 }} /><Box sx={{ flex: 1 }} /><Tooltip title="Raw JSON"><IconButton onClick={() => setRawOpen(true)}><Settings /></IconButton></Tooltip><Tooltip title="Toggle theme"><IconButton onClick={() => setThemeMode(current => current === 'dark' ? 'light' : 'dark')}>{themeMode === 'dark' ? <Brightness7 /> : <Brightness4 />}</IconButton></Tooltip><Button component="label" size="small" startIcon={<Upload />}>Setup<input hidden type="file" accept="application/json" onChange={event => setImport(event.target.files?.[0], 'setup')} /></Button><Button size="small" startIcon={<Download />} onClick={() => download('SetupData.patch.json', { version: 1, questions: Object.values(setupPatches) })}>Setup ({Object.keys(setupPatches).length})</Button><Button component="label" size="small" startIcon={<FolderOpen />}>SCT<input hidden type="file" accept="application/json" onChange={event => setImport(event.target.files?.[0], 'sct')} /></Button><Button size="small" startIcon={<Download />} onClick={() => download('Platform_setup.sct.patch.json', { version: 1, suppressIfPatches: disabledSuppressions.sort((a, b) => a - b).map(offset => ({ disable: true, offset })) })}>SCT ({disabledSuppressions.length})</Button></Toolbar></AppBar><Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'minmax(320px, 38%) minmax(0, 1fr)' }, minHeight: 0 }}><Paper square variant="outlined" sx={{ overflow: 'auto', p: 1 }}><SimpleTreeView expandedItems={expanded} onExpandedItemsChange={(_, ids) => setExpanded(ids)} selectedItems={selectedId} onSelectedItemsChange={(_, id) => typeof id === 'string' && index.byId.has(id) && setSelectedId(id)}>{viewerDocument.Formsets.map((formset, formsetIndex) => <TreeItem key={formsetIndex} itemId={`formset:${formsetIndex}`} label={label(formset.Title) || formset.Guid || 'Formset'}>{formset.Forms.map((form, formIndex) => <TreeItem key={formIndex} itemId={`form:${formsetIndex}:${formIndex}`} label={label(form.Title) || `Form ${form.Id ?? '?'}`}>{form.Children.map(renderNode)}</TreeItem>)}</TreeItem>)}</SimpleTreeView></Paper><Box sx={{ overflow: 'auto', p: 2 }}>{inspector}</Box></Box><Drawer anchor="right" open={rawOpen} onClose={() => setRawOpen(false)}><Box sx={{ width: { xs: '100vw', sm: 540 }, p: 2 }}><Typography variant="h6">Raw JSON</Typography><Divider sx={{ my: 1 }} /><pre>{JSON.stringify(selected?.node ?? viewerDocument, null, 2)}</pre></Box></Drawer></Box></ThemeProvider>;
}

function QuestionInspector({ node, patch, defaults, onPatch }: { node: Node; patch?: SetupPatchQuestion; defaults: { value: number; text: string }[]; onPatch: (node: Node, property: keyof SetupPatchQuestion['question'], value: number) => void }) {
  return <Stack spacing={2}><Typography variant="h5">{label(node.Prompt) || node.Opcode}</Typography><Typography color="text.secondary">{label(node.Help)}</Typography><Metadata rows={[['IFR', `0x${node.Source.Offset.toString(16).toUpperCase()} (${node.Source.Length} bytes)`], ['Question ID', node.QuestionId], ['Storage', `VarStore ${node.VarstoreId ?? '?'} / offset ${node.VarOffset ?? '?'}`], ['Range', node.Range ? `${node.Range.min}..${node.Range.max}, step ${node.Range.step}` : undefined]]} />{node.Options.length > 0 && <Paper variant="outlined" sx={{ p: 1.5 }}><Typography variant="subtitle2">OneOf values</Typography>{defaults.map(option => <Chip key={option.value} sx={{ mt: 1, mr: 1 }} label={`${option.value} = ${option.text || 'unnamed'}`} />)}</Paper>}{patch && <Paper variant="outlined" sx={{ p: 1.5 }}><Typography variant="subtitle2" sx={{ mb: 1 }}>SetupData patch</Typography><Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}><TextField label="Access level" type="number" size="small" value={patch.question.accessLevel} onChange={event => onPatch(node, 'accessLevel', Number(event.target.value))} inputProps={{ min: 0, max: 255 }} />{(['failsafe', 'optimal'] as const).map(property => <FormControl key={property} size="small" sx={{ minWidth: 170 }}><InputLabel>{property === 'failsafe' ? 'Failsafe default' : 'Optimal default'}</InputLabel><Select label={property === 'failsafe' ? 'Failsafe default' : 'Optimal default'} value={patch.question[property]} onChange={event => onPatch(node, property, Number(event.target.value))}>{defaults.length ? defaults.map(option => <MenuItem key={option.value} value={option.value}>{option.value} - {option.text || 'unnamed'}</MenuItem>) : <MenuItem value={patch.question[property]}>{patch.question[property]}</MenuItem>}</Select></FormControl>)}</Stack></Paper>}</Stack>;
}

function ConditionInspector({ node, index, navigate, disabled, onDisabled }: { node: Node; index: ReturnType<typeof indexDocument>; navigate: (reference: NodeRef) => void; disabled: boolean; onDisabled: (value: boolean) => void }) {
  const questions = node.Children.flatMap(child => child.NodeType === 'question' ? [child] : child.Children.filter(item => item.NodeType === 'question'));
  const patchable = node.Opcode === 'SuppressIf' && node.Children.length > 0;
  return <Stack spacing={2}><Typography variant="h5">{(node.Effect ?? node.Opcode).toUpperCase()}</Typography><Alert severity={node.Effect === 'suppress' ? 'warning' : 'info'}>{conditionText(node)}</Alert><Paper variant="outlined" sx={{ p: 1.5 }}><Typography variant="subtitle2" sx={{ mb: 1 }}>Expression</Typography><Stack direction="row" flexWrap="wrap" gap={1}>{node.ExpressionOperations.map(expression => <Chip key={expression.Source.Offset} label={expression.QuestionId != null ? <QuestionReference questionId={expression.QuestionId} index={index} navigate={navigate} /> : expressionText(expression)} />)}</Stack></Paper><Paper variant="outlined" sx={{ p: 1.5 }}><Typography variant="subtitle2">Affected visible questions ({questions.length})</Typography>{questions.map(question => <Chip key={question.Source.Offset} sx={{ mt: 1, mr: 1 }} label={label(question.Prompt) || question.Opcode} />)}</Paper>{patchable && <Paper variant="outlined" sx={{ p: 1.5 }}><Stack direction="row" alignItems="center" justifyContent="space-between"><Box><Typography variant="subtitle2">Disable suppression in SCT</Typography><Typography variant="caption" color="text.secondary">Exports an existing IfrSctPatches entry for this IFR offset.</Typography></Box><Switch checked={disabled} onChange={event => onDisabled(event.target.checked)} /></Stack></Paper>}</Stack>;
}

function Metadata({ rows }: { rows: [string, unknown][] }) { return <Paper variant="outlined" sx={{ p: 1.5 }}><Stack spacing={0.75}>{rows.filter(([, value]) => value != null).map(([name, value]) => <Stack key={name} direction="row" spacing={2}><Typography color="text.secondary" sx={{ width: 110 }}>{name}</Typography><Typography>{String(value)}</Typography></Stack>)}</Stack></Paper>; }
