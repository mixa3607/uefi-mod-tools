namespace ArkProjects.UefiModTools.Commands.AmiTools.BiosDecodeCodes;

public static class AmiAptioCodes
{
    public static readonly List<AmiStatusCode> StatusCodes = [];

    static AmiAptioCodes()
    {
        // =========================
        // SEC PHASE
        // =========================
        StatusCodes.Add(new(0x00, "SEC", "Reserved", "Not used"));

        StatusCodes.Add(new(0x01, "SEC", "Progress", "Power on. Reset type detection (soft/hard)."));
        StatusCodes.Add(new(0x02, "SEC", "Progress", "AP initialization before microcode loading"));
        StatusCodes.Add(new(0x03, "SEC", "Progress", "North Bridge initialization before microcode loading"));
        StatusCodes.Add(new(0x04, "SEC", "Progress", "South Bridge initialization before microcode loading"));
        StatusCodes.Add(new(0x05, "SEC", "Progress", "OEM initialization before microcode loading"));
        StatusCodes.Add(new(0x06, "SEC", "Progress", "Microcode loading"));
        StatusCodes.Add(new(0x07, "SEC", "Progress", "AP initialization after microcode loading"));
        StatusCodes.Add(new(0x08, "SEC", "Progress", "North Bridge initialization after microcode loading"));
        StatusCodes.Add(new(0x09, "SEC", "Progress", "South Bridge initialization after microcode loading"));
        StatusCodes.Add(new(0x0A, "SEC", "Progress", "OEM initialization after microcode loading"));
        StatusCodes.Add(new(0x0B, "SEC", "Progress", "Cache initialization"));

        StatusCodes.Add(new(0x0C, "SEC", "Error", "Reserved for future AMI SEC error codes"));
        StatusCodes.Add(new(0x0D, "SEC", "Error", "Reserved for future AMI SEC error codes"));
        StatusCodes.Add(new(0x0E, "SEC", "Error", "Microcode not found"));
        StatusCodes.Add(new(0x0F, "SEC", "Error", "Microcode not loaded"));

        // =========================
        // PEI PHASE – PRE MEMORY
        // =========================
        StatusCodes.Add(new(0x10, "PEI", "Progress", "PEI Core is started"));
        StatusCodes.Add(new(0x11, "PEI", "Progress", "Pre-memory CPU initialization is started"));
        StatusCodes.Add(new(0x12, "PEI", "Progress", "Pre-memory CPU initialization (CPU module specific)"));
        StatusCodes.Add(new(0x13, "PEI", "Progress", "Pre-memory CPU initialization (CPU module specific)"));
        StatusCodes.Add(new(0x14, "PEI", "Progress", "Pre-memory CPU initialization (CPU module specific)"));
        StatusCodes.Add(new(0x15, "PEI", "Progress", "Pre-memory North Bridge initialization is started"));
        StatusCodes.Add(new(0x16, "PEI", "Progress", "Pre-memory North Bridge initialization (module specific)"));
        StatusCodes.Add(new(0x17, "PEI", "Progress", "Pre-memory North Bridge initialization (module specific)"));
        StatusCodes.Add(new(0x18, "PEI", "Progress", "Pre-memory North Bridge initialization (module specific)"));
        StatusCodes.Add(new(0x19, "PEI", "Progress", "Pre-memory South Bridge initialization is started"));
        StatusCodes.Add(new(0x1A, "PEI", "Progress", "Pre-memory South Bridge initialization (module specific)"));
        StatusCodes.Add(new(0x1B, "PEI", "Progress", "Pre-memory South Bridge initialization (module specific)"));
        StatusCodes.Add(new(0x1C, "PEI", "Progress", "Pre-memory South Bridge initialization (module specific)"));

        for (byte i = 0x1D; i <= 0x2A; i++)
            StatusCodes.Add(new(i, "PEI", "OEM", "OEM pre-memory initialization"));

        StatusCodes.Add(new(0x2B, "PEI", "Progress", "Memory initialization. SPD data reading"));
        StatusCodes.Add(new(0x2C, "PEI", "Progress", "Memory initialization. Memory presence detection"));
        StatusCodes.Add(new(0x2D, "PEI", "Progress", "Memory initialization. Programming memory timing"));
        StatusCodes.Add(new(0x2E, "PEI", "Progress", "Memory initialization. Configuring memory"));
        StatusCodes.Add(new(0x2F, "PEI", "Progress", "Memory initialization (other)"));

        StatusCodes.Add(new(0x30, "PEI", "Reserved", "Reserved for ASL"));

        StatusCodes.Add(new(0x31, "PEI", "Progress", "Memory Installed"));
        StatusCodes.Add(new(0x32, "PEI", "Progress", "CPU post-memory initialization started"));
        StatusCodes.Add(new(0x33, "PEI", "Progress", "CPU post-memory cache initialization"));
        StatusCodes.Add(new(0x34, "PEI", "Progress", "CPU post-memory AP initialization"));
        StatusCodes.Add(new(0x35, "PEI", "Progress", "CPU post-memory BSP selection"));
        StatusCodes.Add(new(0x36, "PEI", "Progress", "CPU post-memory SMM initialization"));
        StatusCodes.Add(new(0x37, "PEI", "Progress", "Post-memory North Bridge initialization started"));
        StatusCodes.Add(new(0x38, "PEI", "Progress", "Post-memory North Bridge initialization"));
        StatusCodes.Add(new(0x39, "PEI", "Progress", "Post-memory North Bridge initialization"));
        StatusCodes.Add(new(0x3A, "PEI", "Progress", "Post-memory North Bridge initialization"));
        StatusCodes.Add(new(0x3B, "PEI", "Progress", "Post-memory South Bridge initialization started"));
        StatusCodes.Add(new(0x3C, "PEI", "Progress", "Post-memory South Bridge initialization"));
        StatusCodes.Add(new(0x3D, "PEI", "Progress", "Post-memory South Bridge initialization"));
        StatusCodes.Add(new(0x3E, "PEI", "Progress", "Post-memory South Bridge initialization"));

        for (byte i = 0x3F; i <= 0x4E; i++)
            StatusCodes.Add(new(i, "PEI", "OEM", "OEM post-memory initialization"));

        StatusCodes.Add(new(0x4F, "PEI", "Progress", "DXE IPL is started"));

        // =========================
        // PEI ERRORS
        // =========================
        StatusCodes.Add(new(0x50, "PEI", "Error", "Invalid memory type or incompatible speed"));
        StatusCodes.Add(new(0x51, "PEI", "Error", "SPD reading failed"));
        StatusCodes.Add(new(0x52, "PEI", "Error", "Invalid memory size or mismatch"));
        StatusCodes.Add(new(0x53, "PEI", "Error", "No usable memory detected"));
        StatusCodes.Add(new(0x54, "PEI", "Error", "Unspecified memory initialization error"));
        StatusCodes.Add(new(0x55, "PEI", "Error", "Memory not installed"));
        StatusCodes.Add(new(0x56, "PEI", "Error", "Invalid CPU type or speed"));
        StatusCodes.Add(new(0x57, "PEI", "Error", "CPU mismatch"));
        StatusCodes.Add(new(0x58, "PEI", "Error", "CPU self test failed or cache error"));
        StatusCodes.Add(new(0x59, "PEI", "Error", "CPU microcode not found or update failed"));
        StatusCodes.Add(new(0x5A, "PEI", "Error", "Internal CPU error"));
        StatusCodes.Add(new(0x5B, "PEI", "Error", "Reset PPI not available"));
        StatusCodes.Add(new(0x5C, "PEI", "Error", "PEI phase BMC self-test failure"));

        for (byte i = 0x5D; i <= 0x5F; i++)
            StatusCodes.Add(new(i, "PEI", "Reserved", "Reserved for future AMI error codes"));

        // =========================
        // S3 RESUME
        // =========================
        StatusCodes.Add(new(0xE0, "S3 Resume", "Progress", "S3 Resume started"));
        StatusCodes.Add(new(0xE1, "S3 Resume", "Progress", "S3 Boot Script execution"));
        StatusCodes.Add(new(0xE2, "S3 Resume", "Progress", "Video repost"));
        StatusCodes.Add(new(0xE3, "S3 Resume", "Progress", "OS S3 wake vector call"));

        for (byte i = 0xE4; i <= 0xE7; i++)
            StatusCodes.Add(new(i, "S3 Resume", "Reserved", "Reserved for future AMI progress codes"));

        StatusCodes.Add(new(0xE8, "S3 Resume", "Error", "S3 Resume failed"));
        StatusCodes.Add(new(0xE9, "S3 Resume", "Error", "S3 Resume PPI not found"));
        StatusCodes.Add(new(0xEA, "S3 Resume", "Error", "S3 Resume Boot Script error"));
        StatusCodes.Add(new(0xEB, "S3 Resume", "Error", "S3 OS wake error"));

        for (byte i = 0xEC; i <= 0xEF; i++)
            StatusCodes.Add(new(i, "S3 Resume", "Reserved", "Reserved for future AMI error codes"));

        // =========================
        // RECOVERY
        // =========================
        StatusCodes.Add(new(0xF0, "Recovery", "Progress", "Recovery triggered by firmware"));
        StatusCodes.Add(new(0xF1, "Recovery", "Progress", "Recovery triggered by user"));
        StatusCodes.Add(new(0xF2, "Recovery", "Progress", "Recovery process started"));
        StatusCodes.Add(new(0xF3, "Recovery", "Progress", "Recovery firmware image found"));
        StatusCodes.Add(new(0xF4, "Recovery", "Progress", "Recovery firmware image loaded"));

        for (byte i = 0xF5; i <= 0xF7; i++)
            StatusCodes.Add(new(i, "Recovery", "Reserved", "Reserved for future AMI progress codes"));

        StatusCodes.Add(new(0xF8, "Recovery", "Error", "Recovery PPI not available"));
        StatusCodes.Add(new(0xF9, "Recovery", "Error", "Recovery capsule not found"));
        StatusCodes.Add(new(0xFA, "Recovery", "Error", "Invalid recovery capsule"));

        for (byte i = 0xFB; i < 0xFF; i++)
            StatusCodes.Add(new(i, "Recovery", "Reserved", "Reserved for future AMI error codes"));

        // =========================
        // DXE PHASE
        // =========================
        StatusCodes.Add(new(0x60, "DXE", "Progress", "DXE Core is started"));
        StatusCodes.Add(new(0x61, "DXE", "Progress", "NVRAM initialization"));
        StatusCodes.Add(new(0x62, "DXE", "Progress", "Installation of the South Bridge Runtime Services"));
        StatusCodes.Add(new(0x63, "DXE", "Progress", "CPU DXE initialization is started"));

        for (byte i = 0x64; i <= 0x67; i++)
            StatusCodes.Add(new(i, "DXE", "Progress", "CPU DXE initialization (CPU module specific)"));

        StatusCodes.Add(new(0x68, "DXE", "Progress", "PCI host bridge initialization"));
        StatusCodes.Add(new(0x69, "DXE", "Progress", "North Bridge DXE initialization is started"));
        StatusCodes.Add(new(0x6A, "DXE", "Progress", "North Bridge DXE SMM initialization is started"));

        for (byte i = 0x6B; i <= 0x6F; i++)
            StatusCodes.Add(new(i, "DXE", "Progress", "North Bridge DXE initialization (module specific)"));

        StatusCodes.Add(new(0x70, "DXE", "Progress", "South Bridge DXE initialization is started"));
        StatusCodes.Add(new(0x71, "DXE", "Progress", "South Bridge DXE SMM initialization is started"));
        StatusCodes.Add(new(0x72, "DXE", "Progress", "South Bridge devices initialization"));

        for (byte i = 0x73; i <= 0x77; i++)
            StatusCodes.Add(new(i, "DXE", "Progress", "South Bridge DXE initialization (module specific)"));

        StatusCodes.Add(new(0x78, "DXE", "Progress", "ACPI module initialization"));
        StatusCodes.Add(new(0x79, "DXE", "Progress", "CSM initialization"));

        for (byte i = 0x7A; i <= 0x7F; i++)
            StatusCodes.Add(new(i, "DXE", "Reserved", "Reserved for future AMI DXE codes"));

        for (byte i = 0x80; i <= 0x8F; i++)
            StatusCodes.Add(new(i, "DXE", "OEM", "OEM DXE initialization"));

        // =========================
        // BDS PHASE
        // =========================
        StatusCodes.Add(new(0x90, "BDS", "Progress", "Boot Device Selection phase is started"));
        StatusCodes.Add(new(0x91, "BDS", "Progress", "Driver connecting is started"));
        StatusCodes.Add(new(0x92, "BDS", "Progress", "PCI Bus initialization is started"));
        StatusCodes.Add(new(0x93, "BDS", "Progress", "PCI Bus Hot Plug Controller initialization"));
        StatusCodes.Add(new(0x94, "BDS", "Progress", "PCI Bus enumeration"));
        StatusCodes.Add(new(0x95, "BDS", "Progress", "PCI Bus request resources"));
        StatusCodes.Add(new(0x96, "BDS", "Progress", "PCI Bus assign resources"));
        StatusCodes.Add(new(0x97, "BDS", "Progress", "Console output devices connect"));
        StatusCodes.Add(new(0x98, "BDS", "Progress", "Console input devices connect"));
        StatusCodes.Add(new(0x99, "BDS", "Progress", "Super IO initialization"));

        StatusCodes.Add(new(0x9A, "BDS", "Progress", "USB initialization is started"));
        StatusCodes.Add(new(0x9B, "BDS", "Progress", "USB reset"));
        StatusCodes.Add(new(0x9C, "BDS", "Progress", "USB detect"));
        StatusCodes.Add(new(0x9D, "BDS", "Progress", "USB enable"));

        for (byte i = 0x9E; i <= 0x9F; i++)
            StatusCodes.Add(new(i, "BDS", "Reserved", "Reserved for future AMI codes"));

        StatusCodes.Add(new(0xA0, "BDS", "Progress", "IDE initialization is started"));
        StatusCodes.Add(new(0xA1, "BDS", "Progress", "IDE reset"));
        StatusCodes.Add(new(0xA2, "BDS", "Progress", "IDE detect"));
        StatusCodes.Add(new(0xA3, "BDS", "Progress", "IDE enable"));

        StatusCodes.Add(new(0xA4, "BDS", "Progress", "SCSI initialization is started"));
        StatusCodes.Add(new(0xA5, "BDS", "Progress", "SCSI reset"));
        StatusCodes.Add(new(0xA6, "BDS", "Progress", "SCSI detect"));
        StatusCodes.Add(new(0xA7, "BDS", "Progress", "SCSI enable"));

        StatusCodes.Add(new(0xA8, "BDS", "Progress", "Setup verifying password"));
        StatusCodes.Add(new(0xA9, "BDS", "Progress", "Start of setup"));
        StatusCodes.Add(new(0xAB, "BDS", "Progress", "Setup input wait"));

        StatusCodes.Add(new(0xAD, "BDS", "Progress", "Ready to Boot event"));
        StatusCodes.Add(new(0xAE, "BDS", "Progress", "Legacy Boot event"));
        StatusCodes.Add(new(0xAF, "BDS", "Progress", "Exit Boot Services event"));

        StatusCodes.Add(new(0xB0, "BDS", "Progress", "Runtime Set Virtual Address Map Begin"));
        StatusCodes.Add(new(0xB1, "BDS", "Progress", "Runtime Set Virtual Address Map End"));
        StatusCodes.Add(new(0xB2, "BDS", "Progress", "Legacy Option ROM initialization"));
        StatusCodes.Add(new(0xB3, "BDS", "Progress", "System reset"));
        StatusCodes.Add(new(0xB4, "BDS", "Progress", "USB hot plug"));
        StatusCodes.Add(new(0xB5, "BDS", "Progress", "PCI bus hot plug"));
        StatusCodes.Add(new(0xB6, "BDS", "Progress", "Clean-up of NVRAM"));
        StatusCodes.Add(new(0xB7, "BDS", "Progress", "Configuration reset"));

        for (byte i = 0xB8; i <= 0xBF; i++)
            StatusCodes.Add(new(i, "BDS", "Reserved", "Reserved for future AMI codes"));

        for (byte i = 0xC0; i <= 0xCF; i++)
            StatusCodes.Add(new(i, "BDS", "OEM", "OEM BDS initialization"));

        // =========================
        // DXE ERROR CODES
        // =========================
        StatusCodes.Add(new(0xD0, "DXE", "Error", "CPU initialization error"));
        StatusCodes.Add(new(0xD1, "DXE", "Error", "North Bridge initialization error"));
        StatusCodes.Add(new(0xD2, "DXE", "Error", "South Bridge initialization error"));
        StatusCodes.Add(new(0xD3, "DXE", "Error", "Architectural protocols not available"));
        StatusCodes.Add(new(0xD4, "DXE", "Error", "PCI resource allocation error"));
        StatusCodes.Add(new(0xD5, "DXE", "Error", "No space for Legacy Option ROM"));
        StatusCodes.Add(new(0xD6, "DXE", "Error", "No console output devices found"));
        StatusCodes.Add(new(0xD7, "DXE", "Error", "No console input devices found"));
        StatusCodes.Add(new(0xD8, "DXE", "Error", "Invalid password"));
        StatusCodes.Add(new(0xD9, "DXE", "Error", "Error loading boot option"));
        StatusCodes.Add(new(0xDA, "DXE", "Error", "Boot option failed"));
        StatusCodes.Add(new(0xDB, "DXE", "Error", "Flash update failed"));
        StatusCodes.Add(new(0xDC, "DXE", "Error", "Reset protocol not available"));

        // =========================
        // ACPI / ASL CHECKPOINTS
        // =========================
        StatusCodes.Add(new(0x01, "ACPI/ASL", "Progress", "Entering S1 sleep state"));
        StatusCodes.Add(new(0x02, "ACPI/ASL", "Progress", "Entering S2 sleep state"));
        StatusCodes.Add(new(0x03, "ACPI/ASL", "Progress", "Entering S3 sleep state"));
        StatusCodes.Add(new(0x04, "ACPI/ASL", "Progress", "Entering S4 sleep state"));
        StatusCodes.Add(new(0x05, "ACPI/ASL", "Progress", "Entering S5 sleep state"));

        StatusCodes.Add(new(0x10, "ACPI/ASL", "Progress", "Waking from S1 sleep state"));
        StatusCodes.Add(new(0x20, "ACPI/ASL", "Progress", "Waking from S2 sleep state"));
        StatusCodes.Add(new(0x30, "ACPI/ASL", "Progress", "Waking from S3 sleep state"));
        StatusCodes.Add(new(0x40, "ACPI/ASL", "Progress", "Waking from S4 sleep state"));

        StatusCodes.Add(new(0xAC, "ACPI/ASL", "Progress", "ACPI mode entered (PIC mode)"));
        StatusCodes.Add(new(0xAA, "ACPI/ASL", "Progress", "ACPI mode entered (APIC mode)"));
    }
}
