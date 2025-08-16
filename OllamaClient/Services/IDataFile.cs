using System;

namespace OllamaClient.Services
{
    public interface IDataFile<T> where T : class
    {
        Uri FileUri { get; set; }

        bool Exists();
        T? Get();
        void Set(T obj);
    }
}