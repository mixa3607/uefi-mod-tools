# UEFI Data and IFR Workflow

The `uefi` module covers Intel FIT and microcode data plus an IFR-driven workflow for `Platform_setup.sct` and AMI `SetupData`.

## FIT

### Read and Write

```bash
uefi-mod-tools uefi fit-read --input fit.bin --output fit.json
uefi-mod-tools uefi fit-write --input fit.json --output modified-fit.bin
```

`fit-read` emits the JSON accepted by `fit-write`. Verification is enabled by default and requires a byte-identical repack. Preserve `headGarbage` and `tailGarbage` unless intentionally changing data outside the FIT structure.

The first entry must be `FitHeaderEntry`; its `size` equals the number of FIT entries. Addresses, sizes, versions, and checksums accept hexadecimal strings.

```json
{
  "headGarbage": "",
  "entries": [
    {
      "address": "0x00000000FFFFFFC0",
      "size": "0x00000001",
      "reserved": 0,
      "version": "0x00000100",
      "checksumValidate": true,
      "type": "FitHeaderEntry",
      "checksum": 0
    }
  ],
  "tailGarbage": ""
}
```

### Microcode Injection

```bash
uefi-mod-tools uefi mcodes-combine \
  --input bios.bin --table microcodes.json --mcodes microcodes --output modified-bios.bin

uefi-mod-tools uefi fit-inject-mcodes \
  --input fit.bin --table microcodes.json --mcodes microcodes --output modified-fit.bin
```

Both commands use the same table:

```json
{
  "sectionBaseAddress": "0xFFB00090",
  "usableStart": "0x00000000",
  "usableEnd": "0x00100000",
  "microcodeFiles": ["cpu-000906EA.bin", "cpu-000906EB.bin"]
}
```

Microcode files are resolved relative to `--mcodes`. `sectionBaseAddress` is added to calculated microcode addresses. The usable range is `[usableStart, usableEnd)`; omit `usableEnd` or use `-1` to allow the rest of the input.

## IFR, SetupData, and SCT

This workflow requires three matching inputs:

- an IFR operation JSON dump;
- `Platform_setup.sct`;
- `SetupData` binary.

The tool does not produce the IFR dump itself. Use an IFR extractor compatible with the target firmware, then keep that dump with the SCT and SetupData source used to create it.

### Extract and Patch SetupData

```bash
uefi-mod-tools uefi ifr-setupdata-extract \
  --input SetupData.bin --ifr Platform_setup.ifr.json --output SetupData.patch.json

uefi-mod-tools uefi ifr-setupdata-patch \
  --input SetupData.bin --patch SetupData.patch.json --output modified-SetupData.bin
```

Extraction writes a versioned JSON document containing matching AMI SetupData questions. The patch command applies the listed question metadata directly to the SetupData binary. Use a different output filename to preserve the original.

The supported editable metadata is `accessLevel`, `failsafe`, and `optimal`. Access-level meaning is platform-dependent; a commonly used modding value is `0x05`, but it is not a universal AMI role or a guarantee that a setup item becomes usable.

### Patch Supported SCT Suppressions

```bash
uefi-mod-tools uefi ifr-sct-patch \
  --input Platform_setup.sct \
  --ifr Platform_setup.ifr.json \
  --patch Platform_setup.sct.patch.json \
  --output modified-Platform_setup.sct
```

The patch document names original IFR offsets and only supports disabling patchable non-empty `SuppressIf` scopes:

```json
{
  "version": 1,
  "suppressIfPatches": [
    {
      "disable": true,
      "offset": 88706
    }
  ]
}
```

Conditions can guard a value question, a `Ref` navigation item, or an entire nested scope. Removing a suppression does not bypass checks elsewhere in the firmware.

## IFR Render and Viewer

```bash
uefi-mod-tools uefi ifr-render \
  --input Platform_setup.sct \
  --setup-data SetupData.bin \
  --ifr Platform_setup.ifr.json \
  --format json \
  --output Platform_setup.ifr-render.json
```

`--format json` emits a structured document containing formsets, forms, VarStores, questions, conditions, defaults, IFR source locations, and matched SetupData metadata.

`--format html` embeds the same document in a self-contained React/MUI viewer:

```bash
uefi-mod-tools uefi ifr-render \
  --input Platform_setup.sct \
  --setup-data SetupData.bin \
  --ifr Platform_setup.ifr.json \
  --format html \
  --output ifr-viewer.html
```

The viewer provides:

- searchable IFR tree with FormSet, Form, question, and condition nodes;
- readable condition expressions with QuestionId navigation;
- `Ref` to Form links and reverse form references;
- SetupData/SCT patch editing, import/export, and a Changes review tab;
- logical VarStore offsets and ranges from render JSON, not a physical NVRAM map;
- dark and light themes plus raw JSON inspection;
- directory workspaces when browser capabilities allow it.

The selected theme is stored in browser-local UI preferences. Firmware inputs, loaded workspaces, and patches are not persisted by the viewer.

`ascii-tree` is accepted as a format but is currently unimplemented and exits with status 1 without writing output.

### Serve Locally

```bash
uefi-mod-tools uefi ifr-render \
  --input Platform_setup.sct \
  --setup-data SetupData.bin \
  --ifr Platform_setup.ifr.json \
  --format html \
  --serve 127.0.0.1:4060
```

`--serve` requires `--format html`, binds only to `localhost` or a loopback IP, serves the generated page at `/`, and runs until Ctrl+C. It replaces file output for that invocation.

Opening the viewer from `http://127.0.0.1` allows Chromium's File System Access API, so Save all can write a complete workspace to a selected directory. `file://` and browsers without that API use directory-input and download fallbacks.

### Workspace Files

For an embedded render document, Save all writes patch-only workspace files:

```text
ifr-editor.json
SetupData.patch.json
<name>.sct.patch.json
```

Its manifest names both patch files and deliberately omits `IfrRenderFile`:

```json
{
  "Version": 1,
  "SetupDataPatchFile": "SetupData.patch.json",
  "SctPatchFile": "Platform_setup.sct.patch.json"
}
```

Load directory applies this patch-only workspace to the render document embedded in the HTML viewer.

When the render document was loaded from a directory or through `Load IFR render JSON`, Save all also writes `<name>.ifr-render.json` and adds its name to the manifest:

```json
{
  "Version": 1,
  "SetupDataPatchFile": "SetupData.patch.json",
  "SctPatchFile": "Platform_setup.sct.patch.json",
  "IfrRenderFile": "Platform_setup.ifr-render.json"
}
```

Loading a manifest with `IfrRenderFile` replaces the embedded document before applying patches. Without `ifr-editor.json`, Load directory discovers unambiguous conventional filenames. If multiple candidate render or SCT patch files exist, add the manifest instead of relying on an arbitrary selection.

The browser never patches SCT or SetupData binaries. Export or save patch JSON, then use `ifr-setupdata-patch` and `ifr-sct-patch` to produce binary outputs.
