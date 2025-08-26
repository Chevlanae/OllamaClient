namespace OllamaClient.Services
{
    public interface ISerializeableStorageService
    {
        T? Get<T>() where T : class;
        void Set<T>(T obj) where T : class;
    }
}