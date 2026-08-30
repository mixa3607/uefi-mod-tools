import { useState } from 'react';
import { Box, Chip, Menu, MenuItem, Stack, Typography } from '@mui/material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view';
import { conditionText, label, nodeColor, nodeId, type DocumentIndex } from './modelHelpers';
import { QuestionReference } from './QuestionReference';
import type { Document, Node, NodeRef } from './types';

type IfrTreeProps = {
  document: Document;
  expanded: string[];
  selectedId?: string;
  query: string;
  index: DocumentIndex;
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

    return (
      <TreeItem
        key={id}
        itemId={id}
        id={`tree-${id}`}
        label={
          <Box
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
        {document.Formsets.map((formset, formsetIndex) => (
        <TreeItem
          key={formsetIndex}
          itemId={`formset:${formsetIndex}`}
          label={label(formset.Title) || formset.Guid || 'Formset'}
        >
          {formset.Forms.map((form, formIndex) => (
            <TreeItem
              key={formIndex}
              itemId={`form:${formsetIndex}:${formIndex}`}
              label={label(form.Title) || `Form ${form.Id ?? '?'}`}
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

function collectNodeIds(node: Node): string[] {
  return [nodeId(node), ...node.Children.flatMap(collectNodeIds)];
}
