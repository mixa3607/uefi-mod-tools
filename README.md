# UEFI Mod Tools

![CI](https://img.shields.io/github/actions/workflow/status/mixa3607/uefi-mod-tools/push.yml?branch=master&style=flat-square)
![GitHub Release](https://img.shields.io/github/v/release/mixa3607/uefi-mod-tools?display_name=tag&style=flat-square)
![License](https://img.shields.io/github/license/mixa3607/uefi-mod-tools?style=flat-square)

`uefi-mod-tools` is a command-line toolkit for inspecting and editing firmware-adjacent data without treating a full firmware image as an opaque blob. It covers partitioned binary dumps, SMBIOS tables, AMI BMC backups, U-Boot environments, Intel FIT and microcode data, plus an IFR and SetupData workflow for Aptio-style setup modules.

## Why It Is Useful

- Round-trip focused formats: SMBIOS, FIT, U-Boot environment, and supported patch documents are designed to be read, reviewed, edited, and written back.
- IFR workflow: connect an IFR dump, `Platform_setup.sct`, and `SetupData` into structured JSON or a self-contained React viewer.
- Patch review instead of blind byte edits: edit SetupData metadata and supported `SuppressIf` SCT patches as JSON, inspect Changes, then apply them with the CLI.
- Offline HTML viewer: tree navigation, condition expressions, question references, form references, VarStore offsets, patch import/export, and workspace load/save all work locally.
- Small, focused commands for common firmware tasks rather than a monolithic GUI.

The project does not bypass firmware signing, platform security policy, or runtime checks. Always retain the original image and use a recovery path appropriate to the target platform.

## Quick Start

Download a Linux x64 prebuild, or build from source with .NET 10:

```bash
wget https://github.com/mixa3607/uefi-mod-tools/releases/latest/download/uefi-mod-tools_linux-x64_prebuild.zip
unzip uefi-mod-tools_linux-x64_prebuild.zip
chmod +x uefi-mod-tools
./uefi-mod-tools --help
```

```bash
dotnet build src/ArkProjects.UefiModTools/ArkProjects.UefiModTools.csproj -c Release
dotnet run --project src/ArkProjects.UefiModTools/ArkProjects.UefiModTools.csproj -- --help
```

## IFR Viewer

Given a parsed IFR operation JSON, `Platform_setup.sct`, and matching `SetupData`, generate a portable HTML analysis workspace:

```bash
./uefi-mod-tools uefi ifr-render \
  --input Platform_setup.sct \
  --setup-data SetupData.bin \
  --ifr Platform_setup.ifr.json \
  --format html \
  --output ifr-viewer.html
```

For Chromium File System Access API support, serve the same self-contained page only on loopback:

```bash
./uefi-mod-tools uefi ifr-render \
  --input Platform_setup.sct \
  --setup-data SetupData.bin \
  --ifr Platform_setup.ifr.json \
  --format html \
  --serve 127.0.0.1:4060
```

See [the UEFI and IFR guide](docs/cli/uefi.md) for the render, workspace, SetupData, and SCT patch workflow.

## Command Guides

- [Binary partitions](docs/cli/bin.md)
- [SMBIOS](docs/cli/smbios.md)
- [AMI BMC and POST data](docs/cli/ami.md)
- [U-Boot environments](docs/cli/uboot.md)
- [UEFI, FIT, microcode, IFR, SetupData, and SCT](docs/cli/uefi.md)
- [UEFI-Editor JSON menu rendering](docs/cli/uefi-editor-js.md)

Run `uefi-mod-tools <module> <command> --help` for the exact options in the installed version.

## Safety

- Work on copies. Most write commands overwrite the requested output path.
- Keep extracted JSON alongside its source binary. Several JSON formats preserve data needed for byte-accurate output.
- A valid-looking patch can still create an unusable setup UI. Review conditions and forms before changing access metadata or suppressions.
- The viewer edits JSON patch state only. Apply the exported patches with the CLI after review.
