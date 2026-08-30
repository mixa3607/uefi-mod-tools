import { Chip, Stack, Tooltip, Typography } from '@mui/material';
import { SimpleTreeView, TreeItem } from '@mui/x-tree-view';
import { conditionText, label, nodeId } from './modelHelpers';
import type { Document, Node } from './types';

type IfrTreeProps = {
  document: Document;
  expanded: string[];
  selectedId?: string;
  query: string;
  onExpandedChange: (ids: string[]) => void;
  onSelectedChange: (id: string | undefined) => void;
};

export function IfrTree({
  document,
  expanded,
  selectedId,
  query,
  onExpandedChange,
  onSelectedChange,
}: IfrTreeProps) {
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

    return (
      <TreeItem
        key={id}
        itemId={id}
        id={`tree-${id}`}
        label={
          <Stack direction="row" spacing={0.75} alignItems="center" minWidth={0}>
            <Chip
              size="small"
              color={isCondition ? 'warning' : 'default'}
              label={isCondition ? node.Effect ?? node.Opcode : `QID ${node.QuestionId ?? '?'}`}
            />
            <Tooltip title={title}>
              <Typography variant="body2" noWrap>
                {title}
              </Typography>
            </Tooltip>
          </Stack>
        }
      >
        {node.Children.map(renderNode)}
      </TreeItem>
    );
  };

  return (
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
  );
}
