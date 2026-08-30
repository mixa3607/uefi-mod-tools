import { useState } from 'react';
import {
  Alert,
  Box,
  Button,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Paper,
  Popover,
  Select,
  Stack,
  Switch,
  TextField,
  Typography,
} from '@mui/material';
import { conditionText, expressionText, label, type DocumentIndex, type OptionDefault } from './modelHelpers';
import type { Node, NodeRef, SetupPatchQuestion } from './types';

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

  if (selected.node.NodeType === 'question') {
    return (
      <QuestionInspector
        node={selected.node}
        patch={setupPatch}
        defaults={defaults}
        onPatch={onPatchSetup}
      />
    );
  }

  return (
    <ConditionInspector
      node={selected.node}
      index={index}
      navigate={onNavigate}
      disabled={suppressionDisabled}
      onDisabled={onSuppressionDisabled}
    />
  );
}

function QuestionInspector({
  node,
  patch,
  defaults,
  onPatch,
}: {
  node: Node;
  patch?: SetupPatchQuestion;
  defaults: OptionDefault[];
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
        <TextField
          label="Access level"
          type="number"
          size="small"
          value={patch.question.accessLevel}
          onChange={event => onPatch(node, 'accessLevel', Number(event.target.value))}
          inputProps={{ min: 0, max: 255 }}
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

  return (
    <Stack spacing={2}>
      <Typography variant="h5">{(node.Effect ?? node.Opcode).toUpperCase()}</Typography>
      <Alert severity={node.Effect === 'suppress' ? 'warning' : 'info'}>{conditionText(node)}</Alert>
      <Paper variant="outlined" sx={{ p: 1.5 }}>
        <Typography variant="subtitle2" sx={{ mb: 1 }}>
          Expression
        </Typography>
        <Stack direction="row" flexWrap="wrap" gap={1}>
          {node.ExpressionOperations.map(expression => (
            <Chip
              key={expression.Source.Offset}
              label={
                expression.QuestionId != null ? (
                  <QuestionReference
                    questionId={expression.QuestionId}
                    index={index}
                    navigate={navigate}
                  />
                ) : (
                  expressionText(expression)
                )
              }
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

function QuestionReference({
  questionId,
  index,
  navigate,
}: {
  questionId: number;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
}) {
  const [anchor, setAnchor] = useState<HTMLElement | null>(null);
  const targets = index.questionsById.get(questionId) ?? [];

  return (
    <>
      <Button
        size="small"
        onMouseEnter={event => setAnchor(event.currentTarget)}
        onClick={() => targets[0] && navigate(targets[0])}
      >
        Question #{questionId}
      </Button>
      <Popover
        open={Boolean(anchor)}
        anchorEl={anchor}
        onClose={() => setAnchor(null)}
        disableRestoreFocus
        anchorOrigin={{ vertical: 'bottom', horizontal: 'left' }}
      >
        <Box sx={{ p: 1, maxWidth: 360 }}>
          {targets.length ? (
            targets.map(target => (
              <Button
                key={target.id}
                fullWidth
                sx={{ justifyContent: 'flex-start', textTransform: 'none' }}
                onClick={() => {
                  navigate(target);
                  setAnchor(null);
                }}
              >
                <Stack alignItems="flex-start">
                  <Typography>{label(target.node.Prompt) || target.node.Opcode}</Typography>
                  <Typography variant="caption" color="text.secondary">
                    {target.formTitle}, IFR 0x{target.node.Source.Offset.toString(16).toUpperCase()}
                  </Typography>
                </Stack>
              </Button>
            ))
          ) : (
            <Typography variant="body2">Question is not present in this document.</Typography>
          )}
        </Box>
      </Popover>
    </>
  );
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
