# AMI BMC and POST Data

The `ami` module handles AMI BMC backup containers, FMH scans, and BIOS POST-code reports.

## BMC Backup Containers

```bash
uefi-mod-tools ami bmc-backup-extract --input config.bak --output backup
uefi-mod-tools ami bmc-backup-pack --input backup --output modified-config.bak
```

Extraction verifies the backup signature and writes `backup-info.json` beside the unpacked files. Packing consumes that metadata and exactly the listed relative files, then creates a signed importable container.

Preserve `checkSumKeyIndex` and `isBuggedSha1` in `backup-info.json`. They describe firmware-specific checksum behavior. `--force` on extraction bypasses signature validation only to recover a known-corrupt backup:

```bash
uefi-mod-tools ami bmc-backup-extract --input corrupt-config.bak --output backup --force
```

## FMH Scan

```bash
uefi-mod-tools ami bmc-fmh-scan --input bmc-dump.bin --blk-size 0x10000 --output fmh.json
```

Scans an AMI BMC dump and writes discovered FMH structures as JSON. The default block size is `0x10000`.

## BIOS POST Codes

```bash
uefi-mod-tools ami bios-post-decode --input post-codes.txt --output post-codes.md
```

Input is whitespace-separated hexadecimal byte codes. The output is a Markdown table with code index, phase, group, and description. Standard input and output are supported when `--input` or `--output` is omitted.
