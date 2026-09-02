import { useRef, useState } from 'react';
import {
  AccountTree,
  ArrowDropDown,
  Brightness4,
  Brightness7,
  Download,
  FolderOpen,
  Save,
  Settings,
  Upload,
} from '@mui/icons-material';
import { AppBar, Box, Button, ButtonGroup, IconButton, Menu, MenuItem, TextField, Toolbar, Tooltip, Typography } from '@mui/material';
import type { PatchKind } from './patchHelpers';

type ViewerToolbarProps = {
  query: string;
  themeMode: 'dark' | 'light';
  setupPatchCount: number;
  disabledSuppressionCount: number;
  renderFileName: string;
  directoryAccess: boolean;
  onQueryChange: (query: string) => void;
  onToggleTheme: () => void;
  onOpenRaw: () => void;
  onImport: (file: File | undefined, kind: PatchKind) => void;
  onLoadRender: (file: File | undefined) => void;
  onLoadDirectory: () => void;
  onLoadDirectoryFiles: (files: File[]) => void;
  onSaveAll: () => void;
  onExportRender: () => void;
  onExportSetup: () => void;
  onExportSct: () => void;
};

export function ViewerToolbar(props: ViewerToolbarProps) {
  const [loadAnchor, setLoadAnchor] = useState<HTMLElement>();
  const [saveAnchor, setSaveAnchor] = useState<HTMLElement>();
  const renderInput = useRef<HTMLInputElement>(null);
  const setupInput = useRef<HTMLInputElement>(null);
  const sctInput = useRef<HTMLInputElement>(null);
  const directoryInput = useRef<HTMLInputElement>(null);
  const loadDirectory = () => {
    if (props.directoryAccess) {
      props.onLoadDirectory();
    } else {
      directoryInput.current?.click();
    }
  };
  const closeLoad = () => setLoadAnchor(undefined);
  const closeSave = () => setSaveAnchor(undefined);

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
          value={props.query}
          onChange={event => props.onQueryChange(event.target.value)}
          sx={{ width: 280 }}
        />
        <Box sx={{ flex: 1 }} />
        <ButtonGroup size="small" variant="outlined">
          <Button startIcon={<FolderOpen />} onClick={loadDirectory}>
            Load directory
          </Button>
          <Button aria-label="Load options" onClick={event => setLoadAnchor(event.currentTarget)}>
            <ArrowDropDown />
          </Button>
        </ButtonGroup>
        <ButtonGroup size="small" variant="contained">
          <Button startIcon={<Save />} onClick={props.onSaveAll}>
            Save all
          </Button>
          <Button aria-label="Save options" onClick={event => setSaveAnchor(event.currentTarget)}>
            <ArrowDropDown />
          </Button>
        </ButtonGroup>
        <Tooltip title="Raw JSON">
          <IconButton onClick={props.onOpenRaw}>
            <Settings />
          </IconButton>
        </Tooltip>
        <Tooltip title="Toggle theme">
          <IconButton onClick={props.onToggleTheme}>
            {props.themeMode === 'dark' ? <Brightness7 /> : <Brightness4 />}
          </IconButton>
        </Tooltip>
      </Toolbar>
      <Menu anchorEl={loadAnchor} open={Boolean(loadAnchor)} onClose={closeLoad}>
        <MenuItem onClick={() => { closeLoad(); loadDirectory(); }}>
          <FolderOpen fontSize="small" />&nbsp;Load directory
        </MenuItem>
        <MenuItem onClick={() => { closeLoad(); renderInput.current?.click(); }}>
          <Upload fontSize="small" />&nbsp;Load IFR render JSON
        </MenuItem>
        <MenuItem onClick={() => { closeLoad(); setupInput.current?.click(); }}>
          <Upload fontSize="small" />&nbsp;Load SetupData patch
        </MenuItem>
        <MenuItem onClick={() => { closeLoad(); sctInput.current?.click(); }}>
          <Upload fontSize="small" />&nbsp;Load SCT patch
        </MenuItem>
      </Menu>
      <Menu anchorEl={saveAnchor} open={Boolean(saveAnchor)} onClose={closeSave}>
        <MenuItem onClick={() => { closeSave(); props.onSaveAll(); }}>
          <Save fontSize="small" />&nbsp;Save all
        </MenuItem>
        <MenuItem title={props.renderFileName} onClick={() => { closeSave(); props.onExportRender(); }}>
          <Download fontSize="small" />&nbsp;Save IFR render JSON
        </MenuItem>
        <MenuItem onClick={() => { closeSave(); props.onExportSetup(); }}>
          <Download fontSize="small" />&nbsp;Save SetupData patch ({props.setupPatchCount})
        </MenuItem>
        <MenuItem onClick={() => { closeSave(); props.onExportSct(); }}>
          <Download fontSize="small" />&nbsp;Save SCT patch ({props.disabledSuppressionCount})
        </MenuItem>
      </Menu>
      <input
        hidden
        ref={renderInput}
        type="file"
        accept="application/json"
        onChange={event => props.onLoadRender(event.target.files?.[0])}
      />
      <input
        hidden
        ref={setupInput}
        type="file"
        accept="application/json"
        onChange={event => props.onImport(event.target.files?.[0], 'setup')}
      />
      <input
        hidden
        ref={sctInput}
        type="file"
        accept="application/json"
        onChange={event => props.onImport(event.target.files?.[0], 'sct')}
      />
      <input
        hidden
        ref={directoryInput}
        type="file"
        onClick={event => {
          (event.currentTarget as HTMLInputElement & { webkitdirectory: boolean }).webkitdirectory = true;
        }}
        onChange={event => props.onLoadDirectoryFiles(Array.from(event.target.files ?? []))}
      />
    </AppBar>
  );
}
