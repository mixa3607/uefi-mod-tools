# IFR React Viewer MVP Plan

## Status

- Branch: `feat/ifr-react-viewer-mvp`.
- The current `ifr-render --format json` output contains formsets, forms, question nodes, conditional nodes, IFR source locations, OneOf options/defaults, and matched `AmiSetupDataQuestion` data.
- The current `ifr-render --format html` output is a self-contained React/MUI viewer. Vite source is in `Commands/UefiTools/IfrRender/HtmlViewer/Client/`; the .NET build embeds generated `viewer.js` and `viewer.css` into the HTML shell.
- The React MVP provides tree search/navigation, dark/light themes, question and condition inspectors, QuestionId links/popovers, raw JSON, SetupData editing, and import/export for both existing patch formats.
- FormSet, Form, question, and condition are selectable nodes with IFR source metadata, a readable inspector, raw JSON, and recursive context-menu expand/collapse.
- The tree marks actual SetupData patch changes with a hoverable field-level diff and marks SCT-disabled `SuppressIf` scopes separately.
- `ifr-render --format html --serve 127.0.0.1:4060` serves the self-contained viewer from a loopback-only local HTTP origin for File System Access API support in Chromium.
- The CLI patch formats are already applied by separate commands and must remain unchanged:
  - SetupData: `ExtractedAmiSetupDataQuestions` for `AccessLevel`, `Failsafe`, and `Optimal`.
  - SCT: `IfrSctPatches` for disabling patchable `SuppressIf` scopes by original IFR offset.
- The React MVP implementation is complete. Future work should be evaluated against the non-goals before adding new state, dependencies, or abstractions.

## MVP Goals

1. Replace the vanilla viewer implementation with a React and TypeScript application.
2. Keep `ifr-render --format html` self-contained and usable from a local file without a server.
3. Make question-to-question references in IFR expressions discoverable and navigable.
4. Make conditions understandable as effects applied to a visible set of questions.
5. Let a user import, edit, and export both existing patch formats without editing JSON manually.
6. Make default values for OneOf questions selectable by their actual option text and value.

## UX Decisions

1. Use a dense, technical layout inspired by Visual Studio rather than a card-heavy product UI.
2. Default to a dark theme, with a light-theme toggle in the toolbar.
3. Do not add a separate styling or icon library. Use Material UI theming and its included components/icons only.
4. Use a three-pane desktop layout:
   - Left: searchable IFR tree.
   - Center: selected question or condition inspector.
   - Right: raw JSON/detailed diagnostics drawer, collapsed by default.
5. On narrow screens, panes stack vertically while retaining the same controls.
6. Conditions show a human-readable sentence, their IFR expression operations, and their affected questions.
7. References such as `EqIdVal QuestionId=18` are links. Hover opens a concise popover; click selects, expands, and scrolls to that question in the tree.
8. The browser edits patch state only. It never patches SCT or SetupData binaries directly.

## Technical Decisions

1. Use Vite, React, and TypeScript.
2. Use `@mui/material`, `@mui/icons-material`, and `@mui/x-tree-view`.
3. Use a local React reducer and context for selected node, expanded tree nodes, theme preference, imported patch data, and pending edits. A separate state-management dependency is not justified for the MVP.
4. Use a MUI drawer and a syntax-neutral `pre` for the raw JSON inspector. Add an interactive JSON-view dependency only if the raw diagnostics prove insufficient.
5. Use Vite library mode to emit fixed `viewer.js` and `viewer.css` artifacts. The .NET renderer inlines both into its HTML shell, so production output remains a single file without a Vite single-file plugin.
6. Continue embedding the built frontend in the .NET assembly. `IfrHtmlViewerRenderer` inserts IFR render JSON into the generated page.
7. Build a `QuestionId -> question node` index when the document loads. Do not repeatedly traverse the tree for references.
8. Keep the render JSON immutable; frontend state records selection, expansion, and patch changes separately.
9. Support `ifr-render --format html --serve 127.0.0.1:4060` through a minimal loopback-only `TcpListener` server. It serves the generated single HTML document at `/` and enables reliable File System Access API support in Chromium without Kestrel or HTTPS.

## Components

1. `ViewerApp`: theme, app layout, and global keyboard/navigation behavior.
2. `ViewerToolbar`: search, filters, theme toggle, patch import/export, and dirty counters.
3. `IfrTree`: formset/form/condition/question hierarchy with condition effect badges and QuestionId badges.
4. `QuestionInspector`: readable IFR/SetupData metadata, OneOf values, SetupData editors, and ancestor conditions.
5. `ConditionInspector`: effect summary, expression operation chips, reference links, affected-question list, and SCT disable switch.
6. `QuestionReference`: linked QuestionId with hover popover and tree navigation.
7. `PatchStore`: import, edit, reset, validation, and export for existing SetupData and SCT patch schemas.
8. `RawJsonDrawer`: optional raw selected-node JSON in a MUI drawer.

## Engineering Guidelines

1. Prefer MUI components for standard UI behavior before writing custom controls.
2. Keep application state local to the viewer. Do not add global stores, service layers, repositories, factories, or abstractions without a concrete reuse need.
3. Keep data transforms as small pure functions near the feature that uses them.
4. Do not introduce a backend, network calls, CDN assets, or runtime build step for the generated viewer.
5. Add dependencies only when they eliminate meaningful implementation or maintenance work; do not add libraries that duplicate React or MUI capabilities.

## Implementation Steps

1. Create the Vite React TypeScript frontend under `Commands/UefiTools/IfrRender/HtmlViewer/Frontend/` and add npm build scripts.
2. Add MUI theme definitions for dense dark and light themes; create the application shell and responsive pane layout.
3. Define TypeScript types for the existing render JSON and browser-injected document data.
4. Implement document indexing, tree rendering, search, filters, selected-node navigation, and expanded-node state.
5. Implement Question and Condition inspectors, including readable parameter views and raw JSON drawer.
6. Implement QuestionId links, popovers, and scroll-to-selected-tree-node behavior.
7. Implement SetupData patch store/import/export and editors for `AccessLevel`, `Failsafe`, and `Optimal`; use OneOf options for default selectors.
8. Implement SCT patch store/import/export and disable control only for `SuppressIf` scopes that the existing patcher can apply.
9. Replace the current vanilla embedded resources with the Vite single-file output and update the .NET resource/build integration.
10. Add frontend unit tests for patch mapping, reference indexing, and condition descriptions; retain .NET generation tests.
11. Validate the generated viewer with the real `Platform_setup` fixture, both themes, imports/exports, question navigation, and existing CLI patch commands.
12. Add directory-based load/save after local serve is available. Use `ifr-editor.json` as an optional manifest that names `<name>.ifr-render.json`, `SetupData.patch.json`, and `<name>.sct.patch.json`.

## Non-Goals For MVP

1. Evaluate IFR conditions against a selected runtime SetupData state.
2. Patch SetupData or SCT binaries in the browser.
3. Edit arbitrary IFR opcodes or rebuild IFR bytecode.
4. Add undo/redo, patch history, conflict resolution, or an interactive dependency graph.
5. Add a backend, user accounts, telemetry, or any network-accessible service.

## Acceptance Criteria

1. `ifr-render --format html` emits a single local-file HTML viewer with no network dependency.
2. A user can identify a condition's effect, expression, and affected questions without reading raw JSON.
3. A user can follow an expression reference to its target question and return via normal tree navigation.
4. A user can select real OneOf labels for `Failsafe` and `Optimal`.
5. Exported SetupData JSON is accepted by `ifr-setupdata-patch`.
6. Exported SCT JSON is accepted by `ifr-sct-patch` for supported non-empty `SuppressIf` scopes.
7. Dark and light themes retain readable dense layouts on desktop and narrow screens.
