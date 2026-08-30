# U-Boot Environments

The `uboot` module finds, reads, and writes classic CRC-protected U-Boot environment sections.

## Find Candidate Environments

```bash
uefi-mod-tools uboot env-scan --input firmware.bin --output env-scan.json
```

`env-scan` writes output-only JSON containing `foundEnvPages`, detected ranges, variables, and padding information. `--blk-size` defaults to `0x10000`; `--windows-blks` defaults to `1`. A trailing input fragment smaller than a block is ignored.

## Read, Edit, Write

```bash
uefi-mod-tools uboot env-read --input uboot-env.bin --output uboot-env.json
uefi-mod-tools uboot env-write --input uboot-env.json --output modified-uboot-env.bin
```

The JSON includes:

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

Keep `size` equal to the full environment section. `paddingSize` is the trailing `0xFF` region excluded from CRC32. `hashMatched` is diagnostic; writing recalculates the CRC.
