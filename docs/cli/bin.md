# Binary Partitions

The `bin` module splits a dump into named ranges and injects replacements back into a copy of that dump. It does not discover partitions itself; the partition table is explicit JSON.

## Partition Table

Property names are camelCase. Comments and trailing commas are accepted. Numeric fields accept JSON numbers or hexadecimal strings.

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

`beginAddress` is inclusive; `endAddress` is exclusive. `fileName` is relative to the selected output or partitions directory.

## Split

```bash
uefi-mod-tools bin split \
  --input dump.bin \
  --table partitions.json \
  --output partitions
```

Writes one file for every listed range. Each range must be inside the input file.

## Combine

```bash
uefi-mod-tools bin combine \
  --input dump.bin \
  --table partitions.json \
  --partitions partitions \
  --output modified.bin
```

Copies the input and replaces every listed partition from `--partitions`. A replacement longer than its range fails. A shorter replacement is padded to the range length with `padByte`.

Keep the original dump and inspect the partition table before injection; this module performs deterministic byte replacement, not format-aware firmware validation.
