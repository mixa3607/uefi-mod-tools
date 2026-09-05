namespace ArkProjects.UefiModTools.Services;

public interface ISerializationService
{
    T Deserialize<T>(string jsonString, SerializationFormat format = SerializationFormat.Auto);
    string Serialize(object data, SerializationFormat format = SerializationFormat.Auto);
}
