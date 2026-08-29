namespace ArkProjects.UefiModTools.Commands.UefiTools.Fit;

public enum FitEntryType : byte
{
    /// <summary>
    /// FIT Header Entry
    /// </summary>
    FitHeaderEntry = 0x00,

    /// <summary>
    /// Microcode Update Entry
    /// </summary>
    MicrocodeUpdateEntry = 0x01,

    /// <summary>
    /// Startup AC Module Entry
    /// </summary>
    StartupAcModuleEntry = 0x02,

    /// <summary>
    /// Diagnostic AC Module Entry
    /// </summary>
    DiagnosticAcModuleEntry = 0x03,

    // 0x04 - 0x06 Intel Reserved

    /// <summary>
    /// BIOS Startup Module Entry
    /// </summary>
    BiosStartupModuleEntry = 0x07,

    /// <summary>
    /// TPM Policy Record
    /// </summary>
    TpmPolicyRecord = 0x08,

    /// <summary>
    /// BIOS Policy Record
    /// </summary>
    BiosPolicyRecord = 0x09,

    /// <summary>
    /// TXT Policy Record
    /// </summary>
    TxtPolicyRecord = 0x0A,

    /// <summary>
    /// Key Manifest Record
    /// </summary>
    KeyManifestRecord = 0x0B,

    /// <summary>
    /// Boot Policy Manifest
    /// </summary>
    BootPolicyManifest = 0x0C,

    // 0x0D - 0x0F Intel Reserved

    /// <summary>
    /// CSE Secure Boot
    /// </summary>
    CseSecureBoot = 0x10,

    // 0x11 - 0x2C Intel Reserved

    /// <summary>
    /// Feature Policy Delivery Record
    /// </summary>
    FeaturePolicyDeliveryRecord = 0x2D,

    // 0x2E Intel Reserved

    /// <summary>
    /// JMP $ Debug Policy
    /// </summary>
    JmpDebugPolicy = 0x2F,

    // 0x30 - 0x70 Reserved for Platform Manufacturer Use
    // 0x71 - 0x7E Intel Reserved

    /// <summary>
    /// Unused Entry (skip)
    /// </summary>
    UnusedEntry = 0x7F
}
