using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OllamaClient.Services
{
    /// <summary>
    /// Class for managing a dictionary of DataFile objects, allowing easy access to serialized data files.
    /// </summary>
    internal static class LocalStorage
    {
        private static Uri DirectoryUri { get; set; }
        private static Dictionary<Type, object> Files { get; set; }

        /// <summary>
        /// Constructor for DataFileDictionary, takes a directory URI as the location for the parent directory
        /// </summary>
        /// <param name="dirUri"></param>
        static LocalStorage()
        {
            DirectoryUri = new(Paths.State);
            Files = [];

            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            foreach (string file in Directory.GetFiles(DirectoryUri.LocalPath))
            {
                string fileName = Path.GetFileNameWithoutExtension(file);

                if (currentAssembly.GetType(fileName) is Type fileType)
                {
                    Type genericType = typeof(DataFile<>).MakeGenericType(fileType);

                    if (Activator.CreateInstance(genericType, DirectoryUri) is object instance)
                    {
                        Files.Add(fileType, instance);
                    }
                }
            }
        }

        /// <summary>
        /// Get a saved object with a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T? Get<T>()
        {
            if (Files.ContainsKey(typeof(T)) && Files[typeof(T)] is DataFile<T> dataFile)
            {
                return dataFile.Get();
            }
            else return default;

        }

        /// <summary>
        /// Set a saved object with a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        public static void Set<T>(T obj)
        {
            if (Files.ContainsKey(typeof(T)) && Files[typeof(T)] is DataFile<T> dataFile)
            {
                dataFile.Set(obj);
            }
            else
            {
                DataFile<T> newFile = new(DirectoryUri);
                newFile.Set(obj);
                Files.Add(typeof(T), newFile);
            }
        }
    }
}
