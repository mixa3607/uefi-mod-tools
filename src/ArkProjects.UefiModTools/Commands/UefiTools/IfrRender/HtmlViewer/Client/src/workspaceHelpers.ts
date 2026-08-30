import { importPatch, type PatchKind } from './patchHelpers';
import type { Document, SetupPatchQuestion } from './types';

export type WorkspaceManifest = {
  Version: 1;
  SetupDataPatchFile: string;
  SctPatchFile: string;
  IfrRenderFile?: string;
};

export type Workspace = {
  document: Document;
  renderOrigin: 'embedded' | 'external';
  renderFileName?: string;
  setupPatches?: Record<number, SetupPatchQuestion>;
  disabledSuppressions?: number[];
};

export async function loadWorkspace(files: File[], embeddedDocument: Document): Promise<Workspace> {
  const byName = new Map(files.map(file => [file.name, file]));
  const manifest = byName.has('ifr-editor.json')
    ? parseManifest(await byName.get('ifr-editor.json')!.text())
    : undefined;
  const renderFile = manifest
    ? manifest.IfrRenderFile
      ? selectFile(byName, manifest.IfrRenderFile, file => file.name.endsWith('.ifr-render.json'), 'IFR render')
      : undefined
    : selectOptionalFile(byName, undefined, file => file.name.endsWith('.ifr-render.json'), 'IFR render');
  const document = renderFile ? parseDocument(await renderFile.text()) : embeddedDocument;
  const setupPatches = await loadPatch(
    selectOptionalFile(byName, manifest?.SetupDataPatchFile, file => file.name === 'SetupData.patch.json', 'SetupData patch'),
    'setup',
  );
  const disabledSuppressions = await loadPatch(
    selectOptionalFile(byName, manifest?.SctPatchFile, file => file.name.endsWith('.sct.patch.json'), 'SCT patch'),
    'sct',
  );

  return {
    document,
    renderOrigin: renderFile ? 'external' : 'embedded',
    renderFileName: renderFile?.name,
    setupPatches: setupPatches?.kind === 'setup' ? setupPatches.setupPatches : undefined,
    disabledSuppressions: disabledSuppressions?.kind === 'sct' ? disabledSuppressions.disabledSuppressions : undefined,
  };
}

export function parseDocument(value: string): Document {
  const document: unknown = JSON.parse(value);
  if (
    typeof document !== 'object' ||
    document === null ||
    !Array.isArray((document as { Formsets?: unknown }).Formsets)
  ) {
    throw new Error('Incompatible IFR render document');
  }

  return document as Document;
}

export function workspaceFiles(
  document: Document,
  renderOrigin: Workspace['renderOrigin'],
  renderFileName: string,
  setupPatches: Record<number, SetupPatchQuestion>,
  disabledSuppressions: number[],
) {
  const stem = renderFileName.endsWith('.ifr-render.json')
    ? renderFileName.slice(0, -'.ifr-render.json'.length)
    : 'Platform_setup';
  const names = {
    SetupDataPatchFile: 'SetupData.patch.json',
    SctPatchFile: `${stem}.sct.patch.json`,
    IfrRenderFile: `${stem}.ifr-render.json`,
  };
  const manifest: WorkspaceManifest = {
    Version: 1,
    SetupDataPatchFile: names.SetupDataPatchFile,
    SctPatchFile: names.SctPatchFile,
    ...(renderOrigin === 'external' ? { IfrRenderFile: names.IfrRenderFile } : {}),
  };
  const files: Record<string, unknown> = {
    'ifr-editor.json': manifest,
    [names.SetupDataPatchFile]: { version: 1, questions: Object.values(setupPatches) },
    [names.SctPatchFile]: {
      version: 1,
      suppressIfPatches: [...disabledSuppressions].sort((a, b) => a - b).map(offset => ({ disable: true, offset })),
    },
  };
  if (renderOrigin === 'external') {
    files[names.IfrRenderFile] = document;
  }

  return files;
}

async function loadPatch(file: File | undefined, kind: PatchKind) {
  return file ? importPatch(file, kind) : undefined;
}

function selectFile(byName: Map<string, File>, preferred: string | undefined,
  predicate: (file: File) => boolean, label: string) {
  const file = selectOptionalFile(byName, preferred, predicate, label);
  if (!file) {
    throw new Error(`${label} file was not found.`);
  }

  return file;
}

function selectOptionalFile(byName: Map<string, File>, preferred: string | undefined,
  predicate: (file: File) => boolean, label: string) {
  if (preferred) {
    const file = byName.get(preferred);
    if (!file) {
      throw new Error(`${label} file '${preferred}' from ifr-editor.json was not found.`);
    }

    return file;
  }

  const matches = [...byName.values()].filter(predicate);
  if (matches.length > 1) {
    throw new Error(`Multiple ${label} files were found. Add ifr-editor.json to select one.`);
  }

  return matches[0];
}

function parseManifest(value: string): WorkspaceManifest {
  const manifest: unknown = JSON.parse(value);
  if (
    typeof manifest !== 'object' ||
    manifest === null ||
    (manifest as { Version?: unknown }).Version !== 1 ||
    !['SetupDataPatchFile', 'SctPatchFile'].every(
      key => typeof (manifest as Record<string, unknown>)[key] === 'string',
    ) ||
    ('IfrRenderFile' in manifest && typeof (manifest as { IfrRenderFile?: unknown }).IfrRenderFile !== 'string')
  ) {
    throw new Error('ifr-editor.json is not compatible.');
  }

  return manifest as WorkspaceManifest;
}
