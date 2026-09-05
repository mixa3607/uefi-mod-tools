namespace ArkProjects.UefiModTools.Services.Serialization;

public interface ISerializationService
{
    T Deserialize<T>(string jsonString, SerializationFormat format);
    string Serialize(object data, SerializationFormat format);
}
