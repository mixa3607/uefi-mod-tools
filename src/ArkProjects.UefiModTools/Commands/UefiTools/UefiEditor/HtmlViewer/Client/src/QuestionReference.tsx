import { Box, Button, Link, Paper, Popper, Stack, Typography } from '@mui/material';
import { useRef, useState } from 'react';
import type { DocumentIndex } from './modelHelpers';
import { label } from './modelHelpers';
import type { NodeRef } from './types';

export function QuestionReference({
  questionId,
  index,
  navigate,
  color = 'secondary.main',
}: {
  questionId: number;
  index: DocumentIndex;
  navigate: (reference: NodeRef) => void;
  color?: string;
}) {
  const targets = index.questionsById.get(questionId) ?? [];
  const anchorRef = useRef<HTMLSpanElement>(null);
  const [open, setOpen] = useState(false);

  return (
    <Box
      component="span"
      ref={anchorRef}
      sx={{ display: 'inline-block' }}
      onMouseEnter={() => setOpen(true)}
      onMouseLeave={() => setOpen(false)}
      onClick={event => event.stopPropagation()}
    >
      <Link
        component="button"
        type="button"
        color={color}
        underline="hover"
        aria-expanded={open}
        sx={{ fontWeight: 700, cursor: 'pointer' }}
        onClick={event => {
          event.stopPropagation();
          if (targets[0]) {
            navigate(targets[0]);
          }
        }}
      >
        Question #{questionId}
      </Link>
      <Popper
        open={open}
        anchorEl={anchorRef.current}
        disablePortal
        placement="bottom-start"
        sx={{ zIndex: theme => theme.zIndex.tooltip }}
      >
        <Paper
          elevation={4}
          onClick={event => event.stopPropagation()}
          sx={{ mt: 0.5, maxWidth: 360 }}
        >
          <Box sx={{ p: 1 }}>
            {targets.length ? (
              targets.map(target => (
                <Button
                  key={target.id}
                  fullWidth
                  sx={{ justifyContent: 'flex-start', textTransform: 'none' }}
                  onClick={event => {
                    event.stopPropagation();
                    navigate(target);
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
        </Paper>
      </Popper>
    </Box>
  );
}
