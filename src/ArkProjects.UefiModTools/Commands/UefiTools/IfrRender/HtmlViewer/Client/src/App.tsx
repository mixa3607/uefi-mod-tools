import { useEffect, useMemo, useState } from 'react';
import { Box, CssBaseline, Divider, Drawer, Paper, ThemeProvider, Typography, createTheme } from '@mui/material';
import { IfrTree } from './IfrTree';
import { Inspector } from './Inspector';
import { indexDocument, questionDefaults } from './modelHelpers';
import { downloadJson, importPatch, setupPatch, type PatchKind } from './patchHelpers';
import type { Document, Node, NodeRef, SetupPatchQuestion } from './types';
import { ViewerToolbar } from './ViewerToolbar';

export function App({ document: viewerDocument }: { document: Document }) {
  const index = useMemo(() => indexDocument(viewerDocument), [viewerDocument]);
  const [selectedId, setSelectedId] = useState<string>();
  const [expanded, setExpanded] = useState<string[]>([]);
  const [query, setQuery] = useState('');
  const [themeMode, setThemeMode] = useState<'dark' | 'light'>('dark');
  const [rawOpen, setRawOpen] = useState(false);
  const [setupPatches, setSetupPatches] = useState<Record<number, SetupPatchQuestion>>({});
  const [disabledSuppressions, setDisabledSuppressions] = useState<number[]>([]);

  const theme = useMemo(
    () =>
      createTheme({
        palette: {
          mode: themeMode,
          primary: { main: themeMode === 'dark' ? '#75b5ff' : '#005fb8' },
        },
        typography: { fontSize: 13, fontFamily: 'Segoe UI, system-ui, sans-serif' },
        shape: { borderRadius: 3 },
        components: { MuiPaper: { styleOverrides: { root: { backgroundImage: 'none' } } } },
      }),
    [themeMode],
  );
  const selected = selectedId ? index.byId.get(selectedId) : undefined;

  useEffect(() => {
    if (!selectedId) {
      setSelectedId(index.byId.keys().next().value);
    }
  }, [index, selectedId]);

  const navigate = (reference: NodeRef) => {
    setExpanded(current => [...new Set([...current, ...reference.parentIds])]);
    setSelectedId(reference.id);
    setTimeout(() => {
      document.getElementById(`tree-${reference.id}`)?.scrollIntoView({ block: 'center' });
    }, 0);
  };

  const patchSetup = (
    node: Node,
    property: keyof SetupPatchQuestion['question'],
    value: number,
  ) => {
    const patch = setupPatch(node, setupPatches);

    setSetupPatches(current => ({
      ...current,
      [patch.beginAddress]: {
        ...patch,
        question: { ...patch.question, [property]: value },
      },
    }));
  };

  const handleImport = async (file: File | undefined, kind: PatchKind) => {
    try {
      const imported = await importPatch(file, kind);

      if (imported?.kind === 'setup') {
        setSetupPatches(imported.setupPatches);
      } else if (imported?.kind === 'sct') {
        setDisabledSuppressions(imported.disabledSuppressions);
      }
    } catch {
      alert('The selected file is not a compatible patch.');
    }
  };

  const selectedNode = selected?.node;
  const selectedSetupPatch = selectedNode?.SetupDataQuestion
    ? setupPatch(selectedNode, setupPatches)
    : undefined;

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Box sx={{ height: '100vh', display: 'grid', gridTemplateRows: '48px minmax(0, 1fr)' }}>
        <ViewerToolbar
          query={query}
          themeMode={themeMode}
          setupPatchCount={Object.keys(setupPatches).length}
          disabledSuppressionCount={disabledSuppressions.length}
          onQueryChange={setQuery}
          onToggleTheme={() => setThemeMode(current => (current === 'dark' ? 'light' : 'dark'))}
          onOpenRaw={() => setRawOpen(true)}
          onImport={handleImport}
          onExportSetup={() =>
            downloadJson('SetupData.patch.json', {
              version: 1,
              questions: Object.values(setupPatches),
            })
          }
          onExportSct={() =>
            downloadJson('Platform_setup.sct.patch.json', {
              version: 1,
              suppressIfPatches: [...disabledSuppressions]
                .sort((a, b) => a - b)
                .map(offset => ({ disable: true, offset })),
            })
          }
        />
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', md: 'minmax(320px, 38%) minmax(0, 1fr)' },
            minHeight: 0,
          }}
        >
          <Paper square variant="outlined" sx={{ overflow: 'auto', p: 1 }}>
            <IfrTree
              document={viewerDocument}
              expanded={expanded}
              selectedId={selectedId}
              query={query}
              onExpandedChange={setExpanded}
              onSelectedChange={id => {
                if (id && index.byId.has(id)) {
                  setSelectedId(id);
                }
              }}
            />
          </Paper>
          <Box sx={{ overflow: 'auto', p: 2 }}>
            <Inspector
              selected={selected}
              index={index}
              setupPatch={selectedSetupPatch}
              defaults={selectedNode ? questionDefaults(selectedNode) : []}
              suppressionDisabled={Boolean(
                selectedNode && disabledSuppressions.includes(selectedNode.Source.Offset),
              )}
              onNavigate={navigate}
              onPatchSetup={patchSetup}
              onSuppressionDisabled={disabled => {
                if (!selectedNode) {
                  return;
                }

                setDisabledSuppressions(current =>
                  disabled
                    ? [...new Set([...current, selectedNode.Source.Offset])]
                    : current.filter(offset => offset !== selectedNode.Source.Offset),
                );
              }}
            />
          </Box>
        </Box>
      </Box>
      <Drawer anchor="right" open={rawOpen} onClose={() => setRawOpen(false)}>
        <Box sx={{ width: { xs: '100vw', sm: 540 }, p: 2 }}>
          <Typography variant="h6">Raw JSON</Typography>
          <Divider sx={{ my: 1 }} />
          <pre>{JSON.stringify(selected?.node ?? viewerDocument, null, 2)}</pre>
        </Box>
      </Drawer>
    </ThemeProvider>
  );
}
