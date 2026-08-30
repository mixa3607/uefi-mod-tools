import { useState } from 'react';
import { Box, Chip, Menu, MenuItem, Stack, Tooltip, Typography } from '@mui/material';
import { EditNote, RemoveCircleOutline } from '@mui/icons-material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view';
import { conditionText, label, nodeColor, nodeId, type DocumentIndex } from './modelHelpers';
import { QuestionReference } from './QuestionReference';
import { setupPatchChanges } from './patchHelpers';
import type { Document, Form, Formset, Node, NodeRef, SelectableNode, SetupPatchQuestion } from './types';

type IfrTreeProps = {
  document: Document;
  expanded: string[];
  selectedId?: string;
  query: string;
  index: DocumentIndex;
  setupPatches: Record<number, SetupPatchQuestion>;
  disabledSuppressions: number[];
  onExpandedChange: (ids: string[]) => void;
  onSelectedChange: (id: string | undefined) => void;
  onNavigate: (reference: NodeRef) => void;
};

export function IfrTree({
  document,
  expanded,
  selectedId,
  query,
  index,
  setupPatches,
  disabledSuppressions,
  onExpandedChange,
  onSelectedChange,
  onNavigate,
}: IfrTreeProps) {
  const [contextMenu, setContextMenu] = useState<{
    mouseX: number;
    mouseY: number;
    nodeIds: string[];
  }>();
  const matches = (node: Node): boolean =>
    !query ||
    JSON.stringify(node).toLowerCase().includes(query.toLowerCase()) ||
    node.Children.some(matches);

  const matchesForm = (form: Form) =>
    !query || JSON.stringify(form).toLowerCase().includes(query.toLowerCase()) || form.Children.some(matches);

  const matchesFormset = (formset: Formset) =>
    !query || JSON.stringify(formset).toLowerCase().includes(query.toLowerCase()) || formset.Forms.some(matchesForm);

  const structureLabel = (node: Form | Formset) => (
    <Box
      onClick={event => {
        event.stopPropagation();
        onSelectedChange(nodeId(node));
      }}
      onContextMenu={event => {
        event.preventDefault();
        setContextMenu({ mouseX: event.clientX, mouseY: event.clientY, nodeIds: collectNodeIds(node) });
      }}
    >
      <Stack direction="row" spacing={0.75} alignItems="center" minWidth={0}>
        <Chip size="small" color="primary" label={node.NodeType.toUpperCase()} />
        <Typography variant="body2" noWrap>
          {node.NodeType === 'formset'
            ? label(node.Title) || node.Guid || 'Formset'
            : label(node.Title) || `Form ${node.Id ?? '?'}`}
        </Typography>
      </Stack>
    </Box>
  );

  const renderNode = (node: Node) => {
    if (!matches(node)) {
      return null;
    }

    const id = nodeId(node);
    const isCondition = node.NodeType === 'condition';
    const title = isCondition
      ? conditionText(node)
      : label(node.Prompt) || `${node.Opcode} #${node.QuestionId ?? '?'}`;
    const nodeIds = collectNodeIds(node);
    const changes = node.NodeType === 'question' && node.SetupDataQuestion
      ? setupPatchChanges(node, setupPatches[node.SetupDataQuestion.BeginAddress])
      : [];
    const suppressionDisabled =
      node.NodeType === 'condition' &&
      node.Opcode === 'SuppressIf' &&
      disabledSuppressions.includes(node.Source.Offset);

    return (
      <TreeItem
        key={id}
        itemId={id}
        id={`tree-${id}`}
        label={
          <Box
            onClick={event => {
              event.stopPropagation();
              onSelectedChange(id);
            }}
            onContextMenu={event => {
              event.preventDefault();
              setContextMenu({ mouseX: event.clientX, mouseY: event.clientY, nodeIds });
            }}
          >
            <Stack direction="row" spacing={0.75} alignItems="center" minWidth={0}>
              <Chip
                size="small"
                color={nodeColor(node)}
                label={isCondition ? node.Effect ?? node.Opcode : `QID ${node.QuestionId ?? '?'}`}
              />
              {isCondition ? (
                <ConditionLabel node={node} index={index} navigate={onNavigate} />
              ) : (
                <Typography variant="body2" noWrap>
                  {title}
                </Typography>
              )}
              {changes.length > 0 && <PatchMarker changes={changes} />}
              {suppressionDisabled && <SuppressionMarker />}
            </Stack>
          </Box>
        }
      >
        {node.Children.map(renderNode)}
      </TreeItem>
    );
  };

  return (
    <>
      <SimpleTreeView
        expandedItems={expanded}
        onExpandedItemsChange={(_, ids) => onExpandedChange(ids)}
        selectedItems={selectedId}
        onSelectedItemsChange={(_, id) => onSelectedChange(id ?? undefined)}
      >
        {document.Formsets.filter(matchesFormset).map(formset => (
        <TreeItem
          key={nodeId(formset)}
          itemId={nodeId(formset)}
          id={`tree-${nodeId(formset)}`}
          label={structureLabel(formset)}
        >
          {formset.Forms.filter(matchesForm).map(form => (
            <TreeItem
              key={nodeId(form)}
              itemId={nodeId(form)}
              id={`tree-${nodeId(form)}`}
              label={structureLabel(form)}
            >
              {form.Children.map(renderNode)}
            </TreeItem>
          ))}
        </TreeItem>
        ))}
      </SimpleTreeView>
      <Menu
        open={Boolean(contextMenu)}
        onClose={() => setContextMenu(undefined)}
        anchorReference="anchorPosition"
        anchorPosition={contextMenu ? { top: contextMenu.mouseY, left: contextMenu.mouseX } : undefined}
      >
        <MenuItem
          onClick={() => {
            if (contextMenu) {
              onExpandedChange([...new Set([...expanded, ...contextMenu.nodeIds])]);
            }
            setContextMenu(undefined);
          }}
        >
          Expand nested
        </MenuItem>
        <MenuItem
          onClick={() => {
            if (contextMenu) {
              onExpandedChange(expanded.filter(id => !contextMenu.nodeIds.slice(1).includes(id)));
            }
            setContextMenu(undefined);
          }}
        >
          Collapse nested
        </MenuItem>
      </Menu>
    </>
  );
}

function PatchMarker({ changes }: { changes: ReturnType<typeof setupPatchChanges> }) {
  return (
    <Tooltip
      arrow
      title={
        <Stack spacing={0.25} sx={{ p: 0.25 }}>
          {changes.map(change => (
            <Typography key={change.label} variant="caption">
              {change.label}: {change.original} -&gt; {change.patched}
            </Typography>
          ))}
        </Stack>
      }
    >
      <EditNote color="warning" fontSize="small" />
    </Tooltip>
  );
}

function SuppressionMarker() {
  return (
    <Tooltip arrow title="Suppression disabled in SCT patch">
      <RemoveCircleOutline color="error" fontSize="small" />
    </Tooltip>
  );
}

function ConditionLabel({
  node,
  index,
  navigate,
}: {
  node: Node;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
}) {
  const expression = node.ExpressionOperations[0];

  if (expression?.Opcode === 'EqIdVal' && expression.QuestionId != null) {
    return (
      <Stack direction="row" spacing={0.5} alignItems="center" minWidth={0}>
        <Typography variant="body2">when</Typography>
        <QuestionReference questionId={expression.QuestionId} index={index} navigate={navigate} color="success.main" />
        <Typography variant="body2">equals {String(expression.Value)}</Typography>
      </Stack>
    );
  }

  if (
    expression?.Opcode === 'EqIdId' &&
    expression.QuestionId != null &&
    expression.OtherQuestionId != null
  ) {
    return (
      <Stack direction="row" spacing={0.5} alignItems="center" minWidth={0}>
        <Typography variant="body2">when</Typography>
        <QuestionReference questionId={expression.QuestionId} index={index} navigate={navigate} color="success.main" />
        <Typography variant="body2">equals</Typography>
        <QuestionReference questionId={expression.OtherQuestionId} index={index} navigate={navigate} color="success.main" />
      </Stack>
    );
  }

  return <Typography variant="body2" noWrap>{conditionText(node)}</Typography>;
}

function collectNodeIds(node: SelectableNode): string[] {
  const children = node.NodeType === 'formset' ? node.Forms : node.Children;
  return [nodeId(node), ...children.flatMap(collectNodeIds)];
}
