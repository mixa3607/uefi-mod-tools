import { Box, Button, Link, Paper, Popper, Stack, Typography } from '@mui/material';
import { useRef, useState } from 'react';
import { label, type DocumentIndex } from './modelHelpers';
import type { NodeRef } from './types';

export function FormReference({ formId, index, navigate }: {
  formId: number;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
}) {
  const targets = index.formsById.get(formId) ?? [];
  const anchorRef = useRef<HTMLSpanElement>(null);
  const [open, setOpen] = useState(false);

  return (
    <Box component="span" ref={anchorRef} sx={{ display: 'inline-block' }} onMouseEnter={() => setOpen(true)} onMouseLeave={() => setOpen(false)}>
      <Link
        component="button"
        type="button"
        color="primary"
        underline="hover"
        aria-expanded={open}
        sx={{ fontWeight: 700, cursor: 'pointer' }}
        onClick={() => targets[0] && navigate(targets[0])}
      >
        Form #{formId}
      </Link>
      <PopperContent open={open} anchor={anchorRef.current} targets={targets} navigate={navigate} />
    </Box>
  );
}

function PopperContent({ open, anchor, targets, navigate }: {
  open: boolean;
  anchor: HTMLElement | null;
  targets: NodeRef[];
  navigate: (reference: NodeRef) => void;
}) {
  return (
    <Popper open={open} anchorEl={anchor} disablePortal placement="bottom-start" sx={{ zIndex: theme => theme.zIndex.tooltip }}>
      <Paper elevation={4} sx={{ mt: 0.5, maxWidth: 360 }}>
        <Box sx={{ p: 1 }}>
          {targets.length ? targets.map(target => (
            <Button key={target.id} fullWidth sx={{ justifyContent: 'flex-start', textTransform: 'none' }} onClick={() => navigate(target)}>
              <Stack alignItems="flex-start">
                <Typography>{label(target.node.NodeType === 'form' ? target.node.Title : undefined) || `Form #${target.node.NodeType === 'form' ? target.node.Id : '?'}`}</Typography>
                <Typography variant="caption" color="text.secondary">IFR 0x{target.node.Source.Offset.toString(16).toUpperCase()}</Typography>
              </Stack>
            </Button>
          )) : <Typography variant="body2">Form is not present in this document.</Typography>}
        </Box>
      </Paper>
    </Popper>
  );
}
