# UEFI bios mod tools

![ci](https://img.shields.io/github/actions/workflow/status/mixa3607/uefi-mod-tools/push.yml?branch=master&style=flat-square)
![GitHub Release](https://img.shields.io/github/v/release/mixa3607/uefi-mod-tools?display_name=tag&style=flat-square)
![license](https://img.shields.io/github/license/mixa3607/uefi-mod-tools?style=flat-square)

## Commands
- `uefi-editor-js` - `BoringBoredom/UEFI-Editor` related tools
  - `render-menu` - Render data.json to tree table
- `bin` - Bin dumps related tools
  - `split` - Split dump by partition table
  - `combine` - Combine/inject partitions to file
- `smbios`         - SMBIOS tables related tools
  - `table2json`     - Parse SMBIOS table to RAW structures
  - `json2table`     - Convert json dump to SMBIOS table bin
  - `known-structs`  - List known structure types and it's status
  - `extract-struct` - Parse SMBIOS.json[--idx] structure to json
  - `inject-struct`  - Inject struct to SMBIOS.json by handler id
- `ami`            - AMI bin dumps related tools
  - `bmc-backup-extract`  - Extract config.bak file exported from BMC web ui with sign verification
  - `bmc-backup-pack`     - Pack and sign files to config.bak that can be imported to BMC
  - `bmc-fmh-scan`        - Scan FMH structures in AMI BMC dump
  - `bios-post-decode`    - Decode BIOS post codes
- `uboot`          - UBoot related tools
  - `env-scan`   - Try find UBoot env section in dump file
  - `env-read`   - Parse UBoot env bin section to json
  - `env-write`  - Write UBoot env bin section from json file

## Install
You may download prebuild from releases page or actions.
- [Download latest linux-x86 release](https://github.com/mixa3607/uefi-mod-tools/releases/latest/download/uefi-mod-tools_linux-x64_prebuild.zip)

```bash
wget https://github.com/mixa3607/uefi-mod-tools/releases/latest/download/uefi-mod-tools_linux-x64_prebuild.zip
unzip uefi-mod-tools_linux-x64_prebuild.zip
chmod +x uefi-mod-tools
./uefi-mod-tools --help
```
