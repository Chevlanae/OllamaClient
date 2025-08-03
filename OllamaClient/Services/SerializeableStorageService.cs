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
        public class Settings
        {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
            public DirectoryOption Directory { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        }

        private readonly ILogger _Logger;
        private readonly string _Directory;
        private Dictionary<Type, object> _Files { get; set; }

        public enum DirectoryOption
        {
            AppData,
            WorkingDirectory
        }

        public SerializeableStorageService(ILogger<SerializeableStorageService> logger, IOptions<Settings> settings)
        {
            _Logger = logger;
            _Files = [];

            _Directory = settings.Value.Directory switch
            {
                DirectoryOption.AppData => $"{App.LocalAppDataPath}\\Storage",
                DirectoryOption.WorkingDirectory => $"{Environment.CurrentDirectory}\\Storage",
                _ => throw new ArgumentException($"Could not parse option {nameof(settings.Value.Directory)}")
            };

            if (!Directory.Exists(_Directory)) Directory.CreateDirectory(_Directory);

            // Get the current assembly. Usually OllamaClient (I hope), pls don't reference my library. It's private! >:( 
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            // Iterate through all files in Paths.State
            foreach (string file in Directory.GetFiles(_Directory))
            {
                // Get the file name without the extension, yay long method names!
                string fileName = Path.GetFileNameWithoutExtension(file);

                // Check if the file name matches a type in the current assembly.
                if (currentAssembly.GetType(fileName) is Type fileType)
                {
                    // Attempt to create an instance of the DataFile<T> class using reflection, where T is the matched type 'fileType'.
                    Type dataFileType = typeof(DataFile<>).MakeGenericType(fileType);

                    // Run the constructor of DataFile<T> and pass DirectoryUri as an argument, then add the constructed instance to the Files dictionary.
                    if (Activator.CreateInstance(dataFileType, new Uri(_Directory)) is object instance)
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
                _Logger.LogDebug("Read {DataFile} at {Uri}", typeof(DataFile<T>).ToString(), dataFile.FileUri.ToString());
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
                _Logger.LogDebug("Wrote {DataFile} to {Uri}", typeof(DataFile<T>).ToString(), dataFile.FileUri.ToString());
                dataFile.Set(obj);
            }
            else
            {
                DataFile<T> newFile = new(new(_Directory));
                _Logger.LogDebug("Wrote {DataFile} to {Uri}", typeof(DataFile<T>).ToString(), newFile.FileUri.ToString());
                newFile.Set(obj);
                _Files.Add(typeof(T), newFile);
            }
        }
    }
}
