import { useEffect, useMemo, useState } from 'react';
import { Box, CssBaseline, Divider, Drawer, Paper, ThemeProvider, Typography, createTheme } from '@mui/material';
import { IfrTree } from './IfrTree';
import { Inspector } from './Inspector';
import { indexDocument, questionDefaults } from './modelHelpers';
import { downloadJson, importPatch, setupPatch, type PatchKind } from './patchHelpers';
import type { Document, Node, NodeRef, SetupPatchQuestion } from './types';
import { ViewerToolbar } from './ViewerToolbar';
import { loadWorkspace, parseDocument, workspaceFiles } from './workspaceHelpers';

type DirectoryHandle = {
  values: () => AsyncIterable<{ kind: string; getFile: () => Promise<File> }>;
  getFileHandle: (name: string, options: { create: true }) => Promise<{
    createWritable: () => Promise<{ write: (data: string) => Promise<void>; close: () => Promise<void> }>;
  }>;
};

type DirectoryWindow = Window & { showDirectoryPicker?: () => Promise<DirectoryHandle> };

export function App({ document: viewerDocument }: { document: Document }) {
  const [activeDocument, setActiveDocument] = useState(viewerDocument);
  const [renderFileName, setRenderFileName] = useState('Platform_setup.ifr-render.json');
  const index = useMemo(() => indexDocument(activeDocument), [activeDocument]);
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
    if (!selectedId || !index.byId.has(selectedId)) {
      setSelectedId(index.byId.keys().next().value);
    }
  }, [index, selectedId]);

  useEffect(() => {
    const normalizedQuery = query.trim().toLowerCase();
    if (!normalizedQuery) {
      return;
    }

    const matchingParents = [...index.byId.values()]
      .filter(reference => JSON.stringify(reference.node).toLowerCase().includes(normalizedQuery))
      .flatMap(reference => reference.parentIds);

    setExpanded(current => [...new Set([...current, ...matchingParents])]);
  }, [index, query]);

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

  const loadRender = async (file: File | undefined) => {
    if (!file) {
      return;
    }

    try {
      setActiveDocument(parseDocument(await file.text()));
      setRenderFileName(file.name);
      setSetupPatches({});
      setDisabledSuppressions([]);
    } catch {
      alert('The selected file is not a compatible IFR render document.');
    }
  };

  const applyWorkspace = (workspace: Awaited<ReturnType<typeof loadWorkspace>>) => {
    setActiveDocument(workspace.document);
    setRenderFileName(workspace.renderFileName);
    setSetupPatches(workspace.setupPatches ?? {});
    setDisabledSuppressions(workspace.disabledSuppressions ?? []);
  };

  const loadDirectoryFiles = async (files: File[]) => {
    try {
      applyWorkspace(await loadWorkspace(files));
    } catch (error) {
      alert(error instanceof Error ? error.message : 'Directory could not be loaded.');
    }
  };

  const directoryAccess = window.isSecureContext && 'showDirectoryPicker' in window;
  const loadDirectory = async () => {
    try {
      const handle = await (window as DirectoryWindow).showDirectoryPicker!();
      const files: File[] = [];
      for await (const entry of handle.values()) {
        if (entry.kind === 'file') {
          files.push(await entry.getFile());
        }
      }

      await loadDirectoryFiles(files);
    } catch (error) {
      if ((error as DOMException).name !== 'AbortError') {
        alert(error instanceof Error ? error.message : 'Directory could not be loaded.');
      }
    }
  };

  const saveAll = async () => {
    const files = workspaceFiles(activeDocument, renderFileName, setupPatches, disabledSuppressions);
    if (!directoryAccess) {
      Object.entries(files).forEach(([name, value]) => downloadJson(name, value));
      return;
    }

    try {
      const handle = await (window as DirectoryWindow).showDirectoryPicker!();
      await Promise.all(Object.entries(files).map(async ([name, value]) => {
        const writable = await (await handle.getFileHandle(name, { create: true })).createWritable();
        await writable.write(JSON.stringify(value, null, 2));
        await writable.close();
      }));
    } catch (error) {
      if ((error as DOMException).name !== 'AbortError') {
        alert(error instanceof Error ? error.message : 'Workspace could not be saved.');
      }
    }
  };

  const selectedNode = selected?.node;
  const selectedSetupPatch =
    selectedNode?.NodeType === 'question' && selectedNode.SetupDataQuestion
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
          renderFileName={renderFileName}
          directoryAccess={directoryAccess}
          onQueryChange={setQuery}
          onToggleTheme={() => setThemeMode(current => (current === 'dark' ? 'light' : 'dark'))}
          onOpenRaw={() => setRawOpen(true)}
          onImport={handleImport}
          onLoadRender={loadRender}
          onLoadDirectory={loadDirectory}
          onLoadDirectoryFiles={loadDirectoryFiles}
          onSaveAll={saveAll}
          onExportRender={() => downloadJson(renderFileName, activeDocument)}
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
              document={activeDocument}
              expanded={expanded}
              selectedId={selectedId}
              query={query}
              index={index}
              setupPatches={setupPatches}
              disabledSuppressions={disabledSuppressions}
              onExpandedChange={setExpanded}
              onSelectedChange={id => {
                if (id && index.byId.has(id)) {
                  setSelectedId(id);
                }
              }}
              onNavigate={navigate}
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
          <pre>{JSON.stringify(selected?.node ?? activeDocument, null, 2)}</pre>
        </Box>
      </Drawer>
    </ThemeProvider>
  );
}
