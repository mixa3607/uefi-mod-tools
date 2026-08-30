import { Button, Chip, Divider, Paper, Stack, Typography } from '@mui/material';
import { ArrowForward, Block } from '@mui/icons-material';
import { label, type DocumentIndex } from './modelHelpers';
import { setupPatchChanges } from './patchHelpers';
import type { Node, NodeRef, SetupPatchQuestion } from './types';

type ChangesViewProps = {
  index: DocumentIndex;
  setupPatches: Record<number, SetupPatchQuestion>;
  disabledSuppressions: number[];
  onNavigate: (reference: NodeRef) => void;
};

export function ChangesView({ index, setupPatches, disabledSuppressions, onNavigate }: ChangesViewProps) {
  const references = [...index.byId.values()];
  const questions = new Map<number, NodeRef>(
    references.flatMap(reference => reference.node.NodeType === 'question' && reference.node.SetupDataQuestion
      ? [[reference.node.SetupDataQuestion.BeginAddress, reference] as const]
      : []),
  );
  const setupChanges = Object.entries(setupPatches).flatMap(([address, patch]) => {
    const reference = questions.get(Number(address));
    return reference && reference.node.NodeType === 'question'
      ? setupPatchChanges(reference.node, patch).map(change => ({ change, reference }))
      : [];
  });
  const suppressions = disabledSuppressions.flatMap(offset => {
    const reference = references.find(item =>
      item.node.NodeType === 'condition' && item.node.Opcode === 'SuppressIf' && item.node.Source.Offset === offset,
    );
    return reference ? [reference] : [];
  });

  return (
    <Stack spacing={2} sx={{ overflow: 'auto', p: 2 }}>
      <Typography variant="h6">Changes</Typography>
      <Typography color="text.secondary" variant="body2">
        Logical changes staged in the loaded workspace. Exported patch JSON remains compatible with the CLI patch commands.
      </Typography>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2">SetupData fields ({setupChanges.length})</Typography>
        <Divider sx={{ my: 1 }} />
        {setupChanges.length === 0 && <Typography color="text.secondary">No changed SetupData fields.</Typography>}
        <Stack spacing={1}>
          {setupChanges.map(({ change, reference }) => (
            <Stack key={`${reference.id}-${change.label}`} direction="row" alignItems="center" spacing={1}>
              <Chip size="small" color="warning" label={change.label} />
              <Typography variant="body2">{label((reference.node as Node).Prompt) || `Question #${(reference.node as Node).QuestionId}`}</Typography>
              <Typography color="text.secondary" variant="body2">{change.original} <ArrowForward fontSize="inherit" /> {change.patched}</Typography>
              <Button size="small" onClick={() => onNavigate(reference)}>Open</Button>
            </Stack>
          ))}
        </Stack>
      </Paper>
      <Paper variant="outlined" sx={{ p: 2 }}>
        <Typography variant="subtitle2">Disabled suppressions ({suppressions.length})</Typography>
        <Divider sx={{ my: 1 }} />
        {suppressions.length === 0 && <Typography color="text.secondary">No SCT suppression changes.</Typography>}
        <Stack spacing={1}>
          {suppressions.map(reference => (
            <Stack key={reference.id} direction="row" alignItems="center" spacing={1}>
              <Block color="error" fontSize="small" />
              <Typography variant="body2">SuppressIf at IFR offset {reference.node.Source.Offset}</Typography>
              <Button size="small" onClick={() => onNavigate(reference)}>Open</Button>
            </Stack>
          ))}
        </Stack>
      </Paper>
    </Stack>
  );
}
