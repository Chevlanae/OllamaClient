using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

namespace OllamaClient.LocalStorage
{
    internal class DataFileDictionary
    {
        private Uri DirectoryUri { get; set; }
        private Dictionary<Type, bool> FileLocks { get; set; }

        public DataFileDictionary(Uri dirUri)
        {
            DirectoryUri = dirUri;
            FileLocks = new();
            LoadLocks();
            Directory.CreateDirectory(DirectoryUri.LocalPath);
        }

        private void LoadLocks()
        {
            //check existing filenames in FolderPath against type names in the currently executing assembly
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (string filename in Directory.GetFiles(DirectoryUri.LocalPath))
            {
                string typeName = Path.GetFileNameWithoutExtension(filename);

                Type? match = assemblyTypes.FirstOrDefault((t) => { return t.Name == typeName; });

                if (match != null)
                {
                    FileLocks[match] = false;
                }
            }
        }

        /// <summary>
        /// Assert that a given type is not primitive, and has the DataContract attribute
        /// </summary>
        /// <param name="T"></param>
        /// <exception cref="ArgumentException"></exception>
        private void AssertType(Type T)
        {
            if (!Attribute.IsDefined(T, typeof(DataContractAttribute)))
            {
                throw new ArgumentException("Given argument was an invalid type. This function only accepts objects with a DataContract attribute", "T");
            }
        }

        /// <summary>
        /// Get a saved object with a given type
        /// </summary>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"</exception>
        /// <returns></returns>
        public async Task<T?> Get<T>()
        {
            Type type = typeof(T);

            AssertType(type);

            if (FileLocks.ContainsKey(type))
            {
                while (FileLocks[type]) await Task.Delay(100);
            }
            else return default;

            string filename = Path.Combine(DirectoryUri.LocalPath, type.Name + ".xml");

            DataContractSerializer serializer = new(type);
            T? result;

            FileLocks[type] = true;

            using (FileStream file = File.OpenRead(filename))
            using (XmlReader reader = XmlReader.Create(file))
            {
                result = (T?)serializer.ReadObject(reader);
            }

            FileLocks[type] = false;

            return result;
        }

        /// <summary>
        /// Save a given object to the appdata\local store
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public async Task Set(object obj)
        {
            Type type = obj.GetType();

            AssertType(type);

            if (FileLocks.ContainsKey(type))
            {
                while (FileLocks[type]) await Task.Delay(100);
            }
            else FileLocks[type] = false;

            string filename = Path.Combine(DirectoryUri.LocalPath, type.Name + ".xml"); ;

            DataContractSerializer serializer = new(type);

            FileLocks[type] = true;

            using (FileStream file = File.Create(filename))
            using (XmlWriter writer = XmlWriter.Create(file))
            {
                serializer.WriteObject(writer, obj);
            }

            FileLocks[type] = false;
        }
    }

    internal static class Paths
    {
        public static string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name ?? "OllamaClient");
    }

    internal static class Persistence
    {
        private static Uri DirectoryUri = new(Path.Combine(Paths.AppData, "Persistence"));
        public static DataFileDictionary Files = new(DirectoryUri);
    }
}
