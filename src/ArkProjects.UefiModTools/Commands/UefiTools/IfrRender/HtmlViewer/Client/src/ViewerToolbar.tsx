import {
  AccountTree,
  Brightness4,
  Brightness7,
  Download,
  FolderOpen,
  Settings,
  Upload,
} from '@mui/icons-material';
import { AppBar, Box, Button, IconButton, TextField, Toolbar, Tooltip, Typography } from '@mui/material';
import type { PatchKind } from './patchHelpers';

type ViewerToolbarProps = {
  query: string;
  themeMode: 'dark' | 'light';
  setupPatchCount: number;
  disabledSuppressionCount: number;
  onQueryChange: (query: string) => void;
  onToggleTheme: () => void;
  onOpenRaw: () => void;
  onImport: (file: File | undefined, kind: PatchKind) => void;
  onExportSetup: () => void;
  onExportSct: () => void;
};

export function ViewerToolbar({
  query,
  themeMode,
  setupPatchCount,
  disabledSuppressionCount,
  onQueryChange,
  onToggleTheme,
  onOpenRaw,
  onImport,
  onExportSetup,
  onExportSct,
}: ViewerToolbarProps) {
  return (
    <AppBar position="static" color="default" elevation={0}>
      <Toolbar variant="dense" sx={{ gap: 1 }}>
        <AccountTree color="primary" />
        <Typography variant="subtitle2" sx={{ mr: 1 }}>
          IFR Viewer
        </Typography>
        <TextField
          size="small"
          placeholder="Search IFR"
          value={query}
          onChange={event => onQueryChange(event.target.value)}
          sx={{ width: 280 }}
        />
        <Box sx={{ flex: 1 }} />
        <Tooltip title="Raw JSON">
          <IconButton onClick={onOpenRaw}>
            <Settings />
          </IconButton>
        </Tooltip>
        <Tooltip title="Toggle theme">
          <IconButton onClick={onToggleTheme}>
            {themeMode === 'dark' ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
        </Tooltip>
        <Button component="label" size="small" startIcon={<Upload />}>
          Setup
          <input
            hidden
            type="file"
            accept="application/json"
            onChange={event => onImport(event.target.files?.[0], 'setup')}
          />
        </Button>
        <Button size="small" startIcon={<Download />} onClick={onExportSetup}>
          Setup ({setupPatchCount})
        </Button>
        <Button component="label" size="small" startIcon={<FolderOpen />}>
          SCT
          <input
            hidden
            type="file"
            accept="application/json"
            onChange={event => onImport(event.target.files?.[0], 'sct')}
          />
        </Button>
        <Button size="small" startIcon={<Download />} onClick={onExportSct}>
          SCT ({disabledSuppressionCount})
        </Button>
      </Toolbar>
    </AppBar>
  );
}
