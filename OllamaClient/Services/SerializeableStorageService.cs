using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    internal class SerializeableStorageService
    {
        public class Settings(Uri directoryUri)
        {
            public Uri DirectoryUri { get; set; } = directoryUri;
        }

        private readonly ILogger _Logger;
        private readonly Settings _Settings;
        private Dictionary<Type, object> _Files { get; set; }

        public SerializeableStorageService(ILogger<SerializeableStorageService> logger, IOptions<Settings> settings)
        {
            _Logger = logger;
            _Settings = settings.Value;
            _Files = [];

            // Get the current assembly. Usually OllamaClient (I hope), pls don't reference my library. It's private! >:( 
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Iterate through all files in Paths.State
            foreach (string file in Directory.GetFiles(_Settings.DirectoryUri.LocalPath))
            {
                // Get the file name without the extension, yay long method names!
                string fileName = Path.GetFileNameWithoutExtension(file);

                // Check if the file name matches a type in the current assembly.
                if (currentAssembly.GetType(fileName) is Type fileType)
                {
                    // Attempt to create an instance of the DataFile<T> class using reflection, where T is the matched type 'fileType'.
                    Type dataFileType = typeof(DataFile<>).MakeGenericType(fileType);

                    // Run the constructor of DataFile<T> and pass DirectoryUri as an argument, then add the constructed instance to the Files dictionary.
                    if (Activator.CreateInstance(dataFileType, _Settings.DirectoryUri) is object instance)
                    {
                        _Files.Add(fileType, instance);
                    }
                }
            }
        }

        /// <summary>
        /// Get a saved object with a given type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? Get<T>()
        {
            // Check if the type exists in the dictionary and if it's value is of type DataFile<T>, then return it. Returns the default value if not.
            if (_Files.ContainsKey(typeof(T)) && _Files[typeof(T)] is DataFile<T> dataFile)
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
        public void Set<T>(T obj)
        {
            // If the type exists in the dictionary and if it's value is of type DataFile<T>, then call DataFile<T>.Set on the value.
            // If not, create a new DataFile<T> instance and add it to the dictionary.
            if (_Files.ContainsKey(typeof(T)) && _Files[typeof(T)] is DataFile<T> dataFile)
            {
                dataFile.Set(obj);
            }
            else
            {
                DataFile<T> newFile = new(_Settings.DirectoryUri);
                newFile.Set(obj);
                _Files.Add(typeof(T), newFile);
            }
        }
    }
}
