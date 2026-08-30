# UEFI-Editor JSON Menu

`uefi-editor-js render-menu` converts a `data.json` document produced by the BoringBoredom UEFI-Editor ecosystem into a readable Markdown tree table.

```bash
uefi-mod-tools uefi-editor-js render-menu --input data.json --output menu.md
```

The output records the navigation path, item type, and `SuppressIf` information. Recursive form references are bounded at depth 10 and self-references stop traversal, so malformed or cyclic menu graphs do not expand indefinitely.

The command is an analysis renderer. It does not modify the original JSON or firmware data.
