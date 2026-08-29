# UEFI bios mod tools

![ci](https://img.shields.io/github/actions/workflow/status/mixa3607/uefi-mod-tools/push.yml?branch=master&style=flat-square)
![GitHub Release](https://img.shields.io/github/v/release/mixa3607/uefi-mod-tools?display_name=tag&style=flat-square)
![license](https://img.shields.io/github/license/mixa3607/uefi-mod-tools?style=flat-square)

## Contents

- [Commands](#commands)
- [Bin tools](#bin-tools)
- [SMBIOS tools](#smbios-tools)
- [AMI tools](#ami-tools)
- [U-Boot tools](#u-boot-tools)
- [UEFI tools](#uefi-tools)
- [Install](#install)

## Commands

- `bin`       - Bin dumps related tools
  - `split`   - Split dump by partition table
  - `combine` - Combine/inject partitions to file
- `smbios`           - SMBIOS tables related tools
  - `table2json`     - Parse SMBIOS table to RAW structures
  - `json2table`     - Convert json dump to SMBIOS table bin
  - `known-structs`  - List known structure types and it's status
  - `extract-struct` - Parse SMBIOS.json[--idx] structure to json
  - `inject-struct`  - Inject struct to SMBIOS.json by handler id
- `ami`                   - AMI bin dumps related tools
  - `bmc-backup-extract`  - Extract config.bak file exported from BMC web ui with sign verification
  - `bmc-backup-pack`     - Pack and sign files to config.bak that can be imported to BMC
  - `bmc-fmh-scan`        - Scan FMH structures in AMI BMC dump
  - `bios-post-decode`    - Decode BIOS post codes
- `uboot`        - UBoot related tools
  - `env-scan`   - Try find UBoot env section in dump file
  - `env-read`   - Parse UBoot env bin section to json
  - `env-write`  - Write UBoot env bin section from json file
- `uefi`             - UEFI related tools
 - `mcodes-combine` - Combine/inject microcodes to file
 - `fit-read`       - Parse FIT bin section to json
 - `fit-write`      - Write FIT bin section from json file
 - `fit-inject-mcodes` - Inject microcodes to FIT file

## Bin tools

`split` and `combine` use a project-specific partition-table JSON. Property names are camelCase; comments and trailing commas are accepted. Hexadecimal fields accept a JSON number or a string such as `"0x00100000"`.

### `split` and `combine`

`--table` is a partition table. `beginAddress` is inclusive and `endAddress` is exclusive. `fileName` is relative to `--output` for `split` and `--partitions` for `combine`; `padByte` fills the unused tail of a short replacement file.

```json
{
  "partitions": [
    {
      "fileName": "bootloader.bin",
      "beginAddress": "0x00000000",
      "endAddress": "0x00060000",
      "padByte": "0xFF"
    }
  ]
}
```

```bash
./uefi-mod-tools bin split -i dump.bin -t partitions.json -o partitions
./uefi-mod-tools bin combine -i dump.bin -t partitions.json -p partitions -o modified.bin
```

## SMBIOS tools

### `table2json`, `json2table`, `extract-struct`, `inject-struct`

`table2json` writes a raw SMBIOS table representation accepted unchanged by `json2table`. Each `body` is Base64-encoded formatted structure data; `strings` contains the SMBIOS string-set. Keep the final `EndOfTable` structure when editing a table.

```json
{
  "length": 256,
  "structures": [
    {
      "structureType": "BiosInformation",
      "structureHandle": 0,
      "strings": ["Example vendor"],
      "body": "AQAAAA=="
    },
    {
      "structureType": "EndOfTable",
      "structureHandle": 65535,
      "strings": [],
      "body": ""
    }
  ]
}
```

```bash
./uefi-mod-tools smbios table2json -i smbios.bin -o smbios-table.json
./uefi-mod-tools smbios json2table -i smbios-table.json -o smbios.bin
./uefi-mod-tools smbios extract-struct -i smbios-table.json --handle 0 -o bios.json
./uefi-mod-tools smbios inject-struct -i smbios-table.json -s bios.json -o modified-table.json
```

`extract-struct` and `inject-struct` use typed JSON only for structures listed by `smbios known-structs`. Generate the structure JSON with `extract-struct`, edit its values, then inject it back.

## AMI tools

### `bmc-backup-extract` and `bmc-backup-pack`

Extraction creates `backup-info.json` next to the unpacked files. Pass that directory to `bmc-backup-pack` after editing the listed files. `checkSumKeyIndex` selects the AMI checksum key; preserve it. `isBuggedSha1` preserves the firmware-specific broken SHA-1 representation when present.

```json
{
  "version": 1,
  "checkSumKeyIndex": 0,
  "isBuggedSha1": false,
  "files": ["config.ini", "users.dat"]
}
```

```bash
./uefi-mod-tools ami bmc-backup-extract -i config.bak -o backup
./uefi-mod-tools ami bmc-backup-pack -i backup -o modified-config.bak
```

Signature validation is mandatory during extraction. Use `--force` only to recover a known corrupt backup:

```bash
./uefi-mod-tools ami bmc-backup-extract -i corrupt-config.bak -o backup --force
```

## U-Boot tools

### `env-read` and `env-write`

`env-read` produces an environment document for `env-write`. Keep `size` equal to the complete environment-section size. `paddingSize` is the trailing `0xFF` region excluded from CRC32; `hashMatched` is diagnostic and is ignored while writing.

```json
{
  "size": 65536,
  "paddingSize": 4,
  "hashMatched": true,
  "variables": {
    "bootdelay": "3",
    "bootcmd": "run boot_sequence"
  }
}
```

```bash
./uefi-mod-tools uboot env-read -i uboot-env.bin -o uboot-env.json
./uefi-mod-tools uboot env-write -i uboot-env.json -o modified-uboot-env.bin
```

`uboot env-scan` returns `foundEnvPages` with the detected ranges, variables, and padding sizes; it is an output-only JSON format.

## UEFI tools

### `mcodes-combine` and `fit-inject-mcodes`

Both commands consume a microcode table through `--table`. `sectionBaseAddress` is added to each calculated microcode address. The usable range is `[usableStart, usableEnd)`; omit `usableEnd` or set it to `-1` to use the end of the input file. File names are resolved relative to `--mcodes`.

```json
{
  "sectionBaseAddress": "0xFFB00090",
  "usableStart": "0x00000000",
  "usableEnd": "0x00100000",
  "microcodeFiles": ["cpu-000906EA.bin", "cpu-000906EB.bin"]
}
```

```bash
./uefi-mod-tools uefi mcodes-combine -i bios.bin -t microcodes.json -m microcodes -o modified-bios.bin
./uefi-mod-tools uefi fit-inject-mcodes -i fit.bin -t microcodes.json -m microcodes -o modified-fit.bin
```

### `fit-read` and `fit-write`

`fit-read` produces the JSON accepted by `fit-write`. Preserve `headGarbage` and `tailGarbage` unless deliberately changing bytes outside FIT. The first entry must have `type` `FitHeaderEntry`; its `size` must equal the number of entries. Addresses, sizes, versions, and checksums accept hexadecimal strings.

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

```bash
./uefi-mod-tools uefi fit-read -i fit.bin -o fit.json
./uefi-mod-tools uefi fit-write -i fit.json -o modified-fit.bin
```

## Install
You may download prebuild from releases page or actions.
- [Download latest linux-x86 release](https://github.com/mixa3607/uefi-mod-tools/releases/latest/download/uefi-mod-tools_linux-x64_prebuild.zip)

```bash
wget https://github.com/mixa3607/uefi-mod-tools/releases/latest/download/uefi-mod-tools_linux-x64_prebuild.zip
unzip uefi-mod-tools_linux-x64_prebuild.zip
chmod +x uefi-mod-tools
./uefi-mod-tools --help
```
