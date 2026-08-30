# SMBIOS

The `smbios` module preserves a raw SMBIOS table in JSON, with optional typed extraction for the structure types supported by the tool.

## Raw Table Round Trip

```bash
uefi-mod-tools smbios table2json --input smbios.bin --output smbios-table.json
uefi-mod-tools smbios json2table --input smbios-table.json --output modified-smbios.bin
```

`table2json` writes a document with Base64 `body` data and a separate SMBIOS `strings` array. Verification is enabled by default and requires a byte-identical JSON-to-binary round trip.

Keep the final `EndOfTable` structure. `json2table` serializes the raw representation; it does not infer missing structures.

## Typed Structure Editing

```bash
uefi-mod-tools smbios known-structs --output known-structs.md
uefi-mod-tools smbios extract-struct \
  --input smbios-table.json --handle 0 --output bios.json
uefi-mod-tools smbios inject-struct \
  --input smbios-table.json --struct bios.json --output modified-table.json
```

`known-structs` lists the structure readers and writers that the installed tool supports. `extract-struct` emits typed JSON only for supported readers. `inject-struct` replaces the same structure handle when it exists, otherwise inserts before `EndOfTable`; it requires a supported writer.

Use raw JSON for lossless retention and typed JSON only for fields the tool can safely model.
