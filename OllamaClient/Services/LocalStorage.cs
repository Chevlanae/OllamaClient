using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml;

namespace OllamaClient.Services
{
    internal static class Paths
    {
        public static string AppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        public static string ParentDirectory = Path.Combine(AppData, "OllamaClient");
        public static string Persistence = Path.Combine(ParentDirectory, "Persistence");
        public static string Logs = Path.Combine(ParentDirectory, "Logs");
        public static string Settings = Path.Combine(ParentDirectory, "Settings");
    }

    namespace Persistence
    {
        /// <summary>
        /// Generic class for encapsulating a DataContract object to a file. Automatically serializes and deserializes the object to/from XML.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal class DataFile<T>
        {
            private Uri FileUri { get; set; }
            private readonly object FileLock = new();
            private readonly DataContractSerializer Serializer = new(typeof(T));
            private readonly Type[] AllowedTypes =
            {
                typeof(ChatItem),
                typeof(Conversation),
                typeof(Conversations)
            };

            /// <summary>
            /// Constructor for DataFile, takes a directory URI as the location for the parent directory, and a type argument for the object to be serialized
            /// </summary>
            /// <param name="dirUri"></param>
            /// <param name="type"></param>
            /// <exception cref="ArgumentNullException"></exception>
            /// <exception cref="ArgumentException"></exception>
            public DataFile(Uri dirUri)
            {
                if (typeof(T) == null)
                {
                    throw new ArgumentNullException($"Given type argument '{nameof(T)}' was null", "T");
                }
                else if (!Attribute.IsDefined(typeof(T), typeof(DataContractAttribute)))
                {
                    throw new ArgumentException($"Given type argument '{nameof(T)}' was an invalid type. The constructor only accepts types with a DataContractAttribute", "T");
                }
                else if (!AllowedTypes.Contains(typeof(T)))
                {
                    throw new ArgumentException($"Given type argument '{nameof(T)}' was an invalid type. The constructor only accepts these types: {String.Join(", ", AllowedTypes.Select(t => t.FullName))}", "T");
                }
                else
                {
                    FileUri = new(Path.Combine(dirUri.LocalPath, typeof(T).FullName + ".xml"));
                }
            }

            /// <summary>
            /// Get a saved object with a given type
            /// </summary>
            /// <param name="type"></param>
            /// <exception cref="ArgumentException"></exception>
            /// <exception cref="FileNotFoundException"</exception>
            /// <returns></returns>
            public T? Get()
            {
                lock (FileLock)
                {
                    using FileStream file = File.OpenRead(FileUri.LocalPath);
                    using XmlReader reader = XmlReader.Create(file);
                    return (T?)Serializer.ReadObject(reader);
                }
            }

            /// <summary>
            /// Save a given object
            /// </summary>
            /// <param name="obj"></param>
            /// <exception cref="ArgumentException"></exception>
            /// <returns></returns>
            public void Set(T obj)
            {
                lock (FileLock)
                {
                    using FileStream file = File.Create(FileUri.LocalPath);
                    using XmlWriter writer = XmlWriter.Create(file);
                    Serializer.WriteObject(writer, obj);
                }
            }
        }

        /// <summary>
        /// Class for managing a dictionary of DataFile objects, allowing easy access to serialized data files.
        /// </summary>
        internal static class DataFileStorage
        {
            private static Uri DirectoryUri { get; set; }
            private static Dictionary<Type, object> Files { get; set; }

            /// <summary>
            /// Constructor for DataFileDictionary, takes a directory URI as the location for the parent directory
            /// </summary>
            /// <param name="dirUri"></param>
            static DataFileStorage()
            {
                DirectoryUri = new(Paths.Persistence);
                Files = [];

                if (!Directory.Exists(DirectoryUri.LocalPath))
                {
                    Directory.CreateDirectory(DirectoryUri.LocalPath);
                }
                else
                {
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
}
