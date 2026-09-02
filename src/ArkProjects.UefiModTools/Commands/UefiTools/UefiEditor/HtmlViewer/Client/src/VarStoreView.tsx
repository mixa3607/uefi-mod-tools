import { useState } from 'react';
import { Box, Button, Chip, Divider, List, ListItemButton, ListItemText, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from '@mui/material';
import { ArrowForward, Storage } from '@mui/icons-material';
import { label } from './modelHelpers';
import type { Document, Node, Varstore } from './types';

type Binding = { node: Node; offset: number; size?: number };
type StoreEntry = { key: string; store: Varstore; bindings: Binding[] };

export function VarStoreView({ document, onOpenQuestion }: { document: Document; onOpenQuestion: (questionId: number) => void }) {
  const stores = collectStores(document);
  const [selectedKey, setSelectedKey] = useState<string>();
  const selected = stores.find(store => store.key === selectedKey) ?? stores[0];

  if (!selected) {
    return <Typography sx={{ p: 2 }} color="text.secondary">The render document does not declare any IFR VarStores.</Typography>;
  }

  const bindings = [...selected.bindings].sort((left, right) => left.offset - right.offset);
  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: '300px minmax(0, 1fr)' }, minHeight: 0 }}>
      <Paper square variant="outlined" sx={{ overflow: 'auto' }}>
        <Typography sx={{ px: 2, pt: 2 }} variant="subtitle2">VarStores ({stores.length})</Typography>
        <List dense>
          {stores.map(entry => (
            <ListItemButton key={entry.key} selected={entry.key === selected.key} onClick={() => setSelectedKey(entry.key)}>
              <Storage color="primary" fontSize="small" sx={{ mr: 1 }} />
              <ListItemText
                primary={entry.store.Name || `VarStore #${entry.store.Id ?? '?'}`}
                secondary={`${entry.store.Size == null ? 'unknown size' : hex(entry.store.Size)} · ${entry.bindings.length} bindings`}
              />
            </ListItemButton>
          ))}
        </List>
      </Paper>
      <Stack spacing={2} sx={{ minWidth: 0, overflow: 'auto', p: 2 }}>
        <Box>
          <Typography variant="h6">{selected.store.Name || `VarStore #${selected.store.Id ?? '?'}`}</Typography>
          <Typography color="text.secondary" variant="body2">
            {selected.store.Kind || 'VarStore'} · {selected.store.Guid || 'no GUID'} · declared size {selected.store.Size == null ? 'unknown' : hex(selected.store.Size)}
          </Typography>
        </Box>
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Typography variant="subtitle2">Offset ruler</Typography>
          <Typography color="text.secondary" variant="caption">Ranges are shown only when IFR supplies a reliable size. Dots are offset anchors.</Typography>
          <OffsetRuler bindings={bindings} size={selected.store.Size} />
        </Paper>
        <Paper variant="outlined" sx={{ overflow: 'auto' }}>
          <Table size="small" stickyHeader>
            <TableHead>
              <TableRow>
                <TableCell>Offset</TableCell>
                <TableCell>Size</TableCell>
                <TableCell>Question</TableCell>
                <TableCell>Type</TableCell>
                <TableCell />
              </TableRow>
            </TableHead>
            <TableBody>
              {bindings.map(binding => (
                <TableRow key={binding.node.Source.Offset} hover>
                  <TableCell>{hex(binding.offset)}</TableCell>
                  <TableCell>{binding.size == null ? '?' : hex(binding.size)}</TableCell>
                  <TableCell>{label(binding.node.Prompt) || `Question #${binding.node.QuestionId ?? '?'}`}</TableCell>
                  <TableCell><Chip size="small" label={binding.node.Opcode} /></TableCell>
                  <TableCell><Button size="small" onClick={() => binding.node.QuestionId != null && onOpenQuestion(binding.node.QuestionId)}>Open <ArrowForward fontSize="inherit" /></Button></TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
          {bindings.length === 0 && <Typography sx={{ p: 2 }} color="text.secondary">No IFR questions reference this VarStore.</Typography>}
        </Paper>
      </Stack>
    </Box>
  );
}

function OffsetRuler({ bindings, size }: { bindings: Binding[]; size?: number }) {
  const denominator = size ?? Math.max(...bindings.map(binding => binding.offset + (binding.size ?? 1)), 1);
  return (
    <Box sx={{ mt: 2 }}>
      <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
        <Typography variant="caption">0x0</Typography><Typography variant="caption">{hex(denominator)}</Typography>
      </Box>
      <Box sx={{ position: 'relative', height: 32, borderRadius: 1, bgcolor: 'action.hover', overflow: 'hidden' }}>
        {bindings.map(binding => {
          const left = Math.min(binding.offset / denominator * 100, 99.5);
          const width = binding.size == null ? 3 : Math.max(binding.size / denominator * 100, 1);
          return <Box key={binding.node.Source.Offset} title={`${label(binding.node.Prompt) || `Question #${binding.node.QuestionId ?? '?'}`} at ${hex(binding.offset)}`}
            sx={{ position: 'absolute', top: binding.size == null ? 11 : 7, left: `${left}%`, width: `${width}%`, height: binding.size == null ? 10 : 18, borderRadius: 1, bgcolor: binding.size == null ? 'info.main' : 'primary.main' }} />;
        })}
      </Box>
    </Box>
  );
}

function collectStores(document: Document): StoreEntry[] {
  return document.Formsets.flatMap(formset => (formset.Varstores ?? []).map(store => ({
    key: `${formset.Guid ?? ''}-${store.Id ?? ''}-${store.Name ?? ''}`,
    store,
    bindings: formset.Forms.flatMap(form => collectNodes(form.Children))
      .filter(node => node.VarstoreId === store.Id && node.VarOffset != null)
      .map(node => ({ node, offset: node.VarOffset!, size: knownSize(node) })),
  })));
}

function collectNodes(nodes: Node[]): Node[] {
  return nodes.flatMap(node => [node, ...collectNodes(node.Children)]);
}

function knownSize(node: Node) {
  if (node.Range?.size_bits && node.Range.size_bits % 8 === 0) {
    return node.Range.size_bits / 8;
  }

  return node.Opcode === 'CheckBox' ? 1 : undefined;
}

function hex(value: number) {
  return `0x${value.toString(16).toUpperCase()}`;
}
