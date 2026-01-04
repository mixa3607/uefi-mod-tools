namespace ArkProjects.UefiModTools.Services;

public interface IJsonSerializationService
{
    T Deserialize<T>(string jsonString);
    string Serialize(object data);
}
