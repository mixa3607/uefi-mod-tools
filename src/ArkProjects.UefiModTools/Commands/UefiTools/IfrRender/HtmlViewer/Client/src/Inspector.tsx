import {
  Alert,
  Autocomplete,
  Box,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { conditionText, expressionText, label, type DocumentIndex, type OptionDefault } from './modelHelpers';
import { QuestionReference } from './QuestionReference';
import { FormReference } from './FormReference';
import type { Form, Formset, Node, NodeRef, SelectableNode, SetupPatchQuestion } from './types';

type InspectorProps = {
  selected?: NodeRef;
  index: DocumentIndex;
  setupPatch?: SetupPatchQuestion;
  defaults: OptionDefault[];
  suppressionDisabled: boolean;
  onNavigate: (reference: NodeRef) => void;
  onPatchSetup: (
    node: Node,
    property: keyof SetupPatchQuestion['question'],
    value: number,
  ) => void;
  onSuppressionDisabled: (disabled: boolean) => void;
};

export function Inspector({
  selected,
  index,
  setupPatch,
  defaults,
  suppressionDisabled,
  onNavigate,
  onPatchSetup,
  onSuppressionDisabled,
}: InspectorProps) {
  if (!selected) {
    return <Typography color="text.secondary">Select a question or condition.</Typography>;
  }

  const node = selected.node;

  if (isStructure(node)) {
    return <StructureInspector node={node} index={index} navigate={onNavigate} />;
  }

  if (node.NodeType === 'question') {
    return (
      <QuestionInspector
        node={node}
        patch={setupPatch}
        defaults={defaults}
        index={index}
        navigate={onNavigate}
        onPatch={onPatchSetup}
      />
    );
  }

  return (
    <ConditionInspector
      node={node}
      index={index}
      navigate={onNavigate}
      disabled={suppressionDisabled}
      onDisabled={onSuppressionDisabled}
    />
  );
}

function isStructure(node: SelectableNode): node is Form | Formset {
  return node.NodeType === 'form' || node.NodeType === 'formset';
}

function StructureInspector({ node, index, navigate }: {
  node: Form | Formset;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
}) {
  const isFormset = node.NodeType === 'formset';
  const title = label(node.Title) || (isFormset ? node.Guid : `Form ${node.Id ?? '?'}`) || 'Unnamed';
  const childCount = isFormset ? node.Forms.length : node.Children.length;
  const references = !isFormset && node.Id != null ? index.referencesByFormId.get(node.Id) ?? [] : [];

  const rows: [string, unknown][] = [
    ['IFR', `0x${node.Source.Offset.toString(16).toUpperCase()} (${node.Source.Length} bytes)`],
    ...(isFormset ? [['GUID', node.Guid] as [string, unknown]] : [['Form ID', node.Id] as [string, unknown]]),
    [isFormset ? 'Forms' : 'Top-level nodes', childCount],
  ];

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{title}</Typography>
      <Typography color="text.secondary">{isFormset ? 'IFR formset' : 'IFR form'}</Typography>
      <Metadata rows={rows} />
      {!isFormset && references.length > 0 && (
        <Paper variant="outlined" sx={{ p: 1.5 }}>
          <Typography variant="subtitle2">Referenced by ({references.length})</Typography>
          {references.map(reference => (
            <Chip
              key={reference.id}
              clickable
              sx={{ mt: 1, mr: 1 }}
              label={label(reference.node.Prompt) || `Question #${reference.node.QuestionId ?? '?'}`}
              onClick={() => navigate(reference)}
            />
          ))}
        </Paper>
      )}
    </Stack>
  );
}

function QuestionInspector({
  node,
  patch,
  defaults,
  index,
  navigate,
  onPatch,
}: {
  node: Node;
  patch?: SetupPatchQuestion;
  defaults: OptionDefault[];
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
  onPatch: (node: Node, property: keyof SetupPatchQuestion['question'], value: number) => void;
}) {
  return (
    <Stack spacing={2}>
      <Typography variant="h5">{label(node.Prompt) || node.Opcode}</Typography>
      <Typography color="text.secondary">{label(node.Help)}</Typography>
      <Metadata
        rows={[
          ['IFR', `0x${node.Source.Offset.toString(16).toUpperCase()} (${node.Source.Length} bytes)`],
          ['Question ID', node.QuestionId],
          ['Storage', `VarStore ${node.VarstoreId ?? '?'} / offset ${node.VarOffset ?? '?'}`],
          ['Range', node.Range ? `${node.Range.min}..${node.Range.max}, step ${node.Range.step}` : undefined],
        ]}
      />
      {node.Opcode === 'Ref' && node.FormId != null && (
        <Paper variant="outlined" sx={{ p: 1.5 }}>
          <Typography variant="subtitle2">Navigation target</Typography>
          <Typography color="text.secondary" variant="body2" sx={{ mb: 0.5 }}>
            This IFR Ref opens another form.
          </Typography>
          <FormReference formId={node.FormId} index={index} navigate={navigate} />
        </Paper>
      )}
      {node.Options.length > 0 && (
        <Paper variant="outlined" sx={{ p: 1.5 }}>
          <Typography variant="subtitle2">OneOf values</Typography>
          {defaults.map(option => (
            <Chip
              key={option.value}
              sx={{ mt: 1, mr: 1 }}
              label={`${option.value} = ${option.text || 'unnamed'}`}
            />
          ))}
        </Paper>
      )}
      {patch && <SetupPatchEditor node={node} patch={patch} defaults={defaults} onPatch={onPatch} />}
    </Stack>
  );
}

function SetupPatchEditor({
  node,
  patch,
  defaults,
  onPatch,
}: {
  node: Node;
  patch: SetupPatchQuestion;
  defaults: OptionDefault[];
  onPatch: (node: Node, property: keyof SetupPatchQuestion['question'], value: number) => void;
}) {
  return (
    <Paper variant="outlined" sx={{ p: 1.5 }}>
      <Typography variant="subtitle2" sx={{ mb: 1 }}>
        SetupData patch
      </Typography>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1}>
        <Autocomplete
          key={patch.beginAddress}
          freeSolo
          options={['0', '1', '2', '3', '4', '5']}
          defaultValue={String(patch.question.accessLevel)}
          onInputChange={(_, value) => {
            const accessLevel = parseAccessLevel(value);
            if (accessLevel != null) {
              onPatch(node, 'accessLevel', accessLevel);
            }
          }}
          sx={{ minWidth: 170 }}
          renderInput={params => (
            <TextField
              {...params}
              label="Access level"
              size="small"
              helperText="0..255, decimal or 0x"
            />
          )}
        />
        {(['failsafe', 'optimal'] as const).map(property => {
          const fieldLabel = property === 'failsafe' ? 'Failsafe default' : 'Optimal default';

          return (
            <FormControl key={property} size="small" sx={{ minWidth: 170 }}>
              <InputLabel>{fieldLabel}</InputLabel>
              <Select
                label={fieldLabel}
                value={patch.question[property]}
                onChange={event => onPatch(node, property, Number(event.target.value))}
              >
                {defaults.length ? (
                  defaults.map(option => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.value} - {option.text || 'unnamed'}
                    </MenuItem>
                  ))
                ) : (
                  <MenuItem value={patch.question[property]}>{patch.question[property]}</MenuItem>
                )}
              </Select>
            </FormControl>
          );
        })}
      </Stack>
    </Paper>
  );
}

function parseAccessLevel(value: string) {
  const normalized = value.trim();
  if (!/^(?:0x[0-9a-f]+|\d+)$/i.test(normalized)) {
    return undefined;
  }

  const parsed = Number(normalized);
  return Number.isInteger(parsed) && parsed >= 0 && parsed <= 0xff ? parsed : undefined;
}

function ConditionInspector({
  node,
  index,
  navigate,
  disabled,
  onDisabled,
}: {
  node: Node;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
  disabled: boolean;
  onDisabled: (value: boolean) => void;
}) {
  const questions = node.Children.flatMap(child =>
    child.NodeType === 'question' ? [child] : child.Children.filter(item => item.NodeType === 'question'),
  );
  const patchable = node.Opcode === 'SuppressIf' && node.Children.length > 0;
  const severity =
    node.Effect === 'suppress'
      ? 'error'
      : node.Effect === 'disable'
        ? 'warning'
        : node.Effect === 'grayout'
          ? 'info'
          : 'success';

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{(node.Effect ?? node.Opcode).toUpperCase()}</Typography>
      <Alert severity={severity}>{conditionText(node)}</Alert>
      <Paper variant="outlined" sx={{ p: 1.5 }}>
        <Typography variant="subtitle2" sx={{ mb: 1 }}>
          Expression
        </Typography>
        <Stack direction="row" flexWrap="wrap" gap={1} alignItems="center">
          {node.ExpressionOperations.map(expression => (
            <ExpressionOperation
              key={expression.Source.Offset}
              expression={expression}
              index={index}
              navigate={navigate}
            />
          ))}
        </Stack>
      </Paper>
      <Paper variant="outlined" sx={{ p: 1.5 }}>
        <Typography variant="subtitle2">Affected visible questions ({questions.length})</Typography>
        {questions.map(question => (
          <Chip
            key={question.Source.Offset}
            sx={{ mt: 1, mr: 1 }}
            label={label(question.Prompt) || question.Opcode}
          />
        ))}
      </Paper>
      {patchable && (
        <Paper variant="outlined" sx={{ p: 1.5 }}>
          <Stack direction="row" alignItems="center" justifyContent="space-between">
            <Box>
              <Typography variant="subtitle2">Disable suppression in SCT</Typography>
              <Typography variant="caption" color="text.secondary">
                Exports an existing IfrSctPatches entry for this IFR offset.
              </Typography>
            </Box>
            <Switch checked={disabled} onChange={event => onDisabled(event.target.checked)} />
          </Stack>
        </Paper>
      )}
    </Stack>
  );
}

function ExpressionOperation({
  expression,
  index,
  navigate,
}: {
  expression: Node['ExpressionOperations'][number];
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
}) {
  if (expression.Opcode === 'EqIdVal' && expression.QuestionId != null) {
    return (
      <Stack direction="row" spacing={0.5} alignItems="center">
        <QuestionReference questionId={expression.QuestionId} index={index} navigate={navigate} />
        <Typography variant="body2">equals {String(expression.Value)}</Typography>
      </Stack>
    );
  }

  if (
    expression.Opcode === 'EqIdId' &&
    expression.QuestionId != null &&
    expression.OtherQuestionId != null
  ) {
    return (
      <Stack direction="row" spacing={0.5} alignItems="center">
        <QuestionReference questionId={expression.QuestionId} index={index} navigate={navigate} />
        <Typography variant="body2">equals</Typography>
        <QuestionReference questionId={expression.OtherQuestionId} index={index} navigate={navigate} />
      </Stack>
    );
  }

  return <Chip label={expressionText(expression)} />;
}

function Metadata({ rows }: { rows: [string, unknown][] }) {
  return (
    <Paper variant="outlined" sx={{ p: 1.5 }}>
      <Stack spacing={0.75}>
        {rows
          .filter(([, value]) => value != null)
          .map(([name, value]) => (
            <Stack key={name} direction="row" spacing={2}>
              <Typography color="text.secondary" sx={{ width: 110 }}>
                {name}
              </Typography>
              <Typography>{String(value)}</Typography>
            </Stack>
          ))}
      </Stack>
    </Paper>
  );
}
