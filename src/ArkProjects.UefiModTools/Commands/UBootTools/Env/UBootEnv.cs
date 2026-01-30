using System.Text.Json.Serialization;
using ArkProjects.UefiModTools.Utils;

namespace ArkProjects.UefiModTools.Commands.UBootTools.Env;

public class UBootEnv
{
    public int Size { get; set; }
    public int PaddingSize { get; set; }
    public bool HashMatched { get; set; }
    public Dictionary<string, string> Variables { get; set; } = [];
}

public class UBootEnvInDump
{
    public Dictionary<string, string> Variables { get; set; } = [];

    [JsonConverter(typeof(NumberConverter<int>))]
    public required int BeginAddress { get; set; }

    [JsonConverter(typeof(NumberConverter<int>))]
    public required int EndAddress { get; set; }

    public required int PaddingSize { get; set; }
}

public class UBootScanResult
{
    public List<UBootEnvInDump> FoundEnvPages { get; set; } = [];
}
