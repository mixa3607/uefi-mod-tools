using System.Text.Json.Serialization;

namespace ArkProjects.UefiModTools.Ifr.Structures;

public class IfrJsonDocument
{
    [JsonPropertyName("program_version")]
    public string ProgramVersion { get; set; } = string.Empty;

    [JsonPropertyName("canonical")]
    public bool Canonical { get; set; }

    [JsonPropertyName("canonical_scope")]
    public string CanonicalScope { get; set; } = string.Empty;

    [JsonPropertyName("extraction_mode")]
    public string ExtractionMode { get; set; } = string.Empty;

    [JsonPropertyName("input_sha256")]
    public string InputSha256 { get; set; } = string.Empty;

    [JsonPropertyName("package_list")]
    public IfrPackageList PackageList { get; set; } = new();

    [JsonPropertyName("form_package")]
    public IfrFormPackage FormPackage { get; set; } = new();

    [JsonPropertyName("string_package")]
    public IfrStringPackage StringPackage { get; set; } = new();

    [JsonPropertyName("operations")]
    public List<IfrOperation> Operations { get; set; } = [];
}
