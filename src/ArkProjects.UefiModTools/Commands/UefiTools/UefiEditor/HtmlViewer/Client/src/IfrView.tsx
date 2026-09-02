import { Box, Paper } from '@mui/material';
import { IfrTree } from './IfrTree';
import { Inspector } from './Inspector';
import { questionDefaults, type DocumentIndex } from './modelHelpers';
import { setupPatch } from './patchHelpers';
import type { Document, Node, NodeRef, SetupPatchQuestion } from './types';

type IfrViewProps = {
  document: Document;
  index: DocumentIndex;
  selectedId?: string;
  expanded: string[];
  query: string;
  setupPatches: Record<number, SetupPatchQuestion>;
  disabledSuppressions: number[];
  onExpandedChange: (ids: string[]) => void;
  onSelectedChange: (id: string | undefined) => void;
  onNavigate: (reference: NodeRef) => void;
  onPatchSetup: (node: Node, property: keyof SetupPatchQuestion['question'], value: number) => void;
  onSuppressionDisabled: (offset: number, disabled: boolean) => void;
};

export function IfrView({
  document,
  index,
  selectedId,
  expanded,
  query,
  setupPatches,
  disabledSuppressions,
  onExpandedChange,
  onSelectedChange,
  onNavigate,
  onPatchSetup,
  onSuppressionDisabled,
}: IfrViewProps) {
  const selected = selectedId ? index.byId.get(selectedId) : undefined;
  const selectedNode = selected?.node;
  const selectedSetupPatch =
    selectedNode?.NodeType === 'question' && selectedNode.SetupDataQuestion
      ? setupPatch(selectedNode, setupPatches)
      : undefined;

  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: { xs: '1fr', md: 'minmax(320px, 38%) minmax(0, 1fr)' },
        minHeight: 0,
      }}
    >
      <Paper square variant="outlined" sx={{ overflow: 'auto', p: 1 }}>
        <IfrTree
          document={document}
          expanded={expanded}
          selectedId={selectedId}
          query={query}
          index={index}
          setupPatches={setupPatches}
          disabledSuppressions={disabledSuppressions}
          onExpandedChange={onExpandedChange}
          onSelectedChange={onSelectedChange}
          onNavigate={onNavigate}
        />
      </Paper>
      <Box sx={{ overflow: 'auto', p: 2 }}>
        <Inspector
          selected={selected}
          index={index}
          setupPatch={selectedSetupPatch}
          defaults={selectedNode?.NodeType === 'question' ? questionDefaults(selectedNode) : []}
          suppressionDisabled={Boolean(
            selectedNode?.NodeType === 'condition' && disabledSuppressions.includes(selectedNode.Source.Offset),
          )}
          onNavigate={onNavigate}
          onPatchSetup={onPatchSetup}
          onSuppressionDisabled={disabled => {
            if (selectedNode) {
              onSuppressionDisabled(selectedNode.Source.Offset, disabled);
            }
          }}
        />
      </Box>
    </Box>
  );
}
