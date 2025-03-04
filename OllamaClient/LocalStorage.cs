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
    internal static class Paths
    {
        public static string AppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetExecutingAssembly().GetName().Name ?? "OllamaClient");
    }

    internal static class Persistence
    {
        /// <summary>
        /// Key represents a type that currently exists, the value is set to true when the file is being accesssed, and false when it is not.
        /// </summary>
        private static Dictionary<Type, bool> Files = new();

        private static string FolderPath = Path.Combine(Paths.AppData, "Persistence");

        static Persistence()
        {
            Directory.CreateDirectory(FolderPath);

            LoadFiles();
        }

        private static void LoadFiles()
        {
            //check existing filenames in FolderPath against type names in the currently executing assembly
            Type[] assemblyTypes = Assembly.GetExecutingAssembly().GetTypes();
            foreach (string filename in Directory.GetFiles(FolderPath))
            {
                string typeName = Path.GetFileNameWithoutExtension(filename);

                Type? match = assemblyTypes.FirstOrDefault((t) => { return t.Name == typeName; });

                if (match != null)
                {
                    Files[match] = false;
                }
            }
        }

        /// <summary>
        /// Assert that a given type is not primitive, and has the DataContract attribute
        /// </summary>
        /// <param name="T"></param>
        /// <exception cref="ArgumentException"></exception>
        private static void AssertType(Type T)
        {
            if (T.IsPrimitive)
            {
                throw new ArgumentException("Given argument was an invalid type. This function does not accept primitive types.", "T");
            }
            else if (!Attribute.IsDefined(T, typeof(DataContractAttribute)))
            {
                throw new ArgumentException("Given argument was an invalid type. This function only accepts objects with a DataContract attribute", "T");
            }
        }

        /// <summary>
        /// Get a saved object with a given type
        /// </summary>
        /// <param name="T"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="FileNotFoundException"</exception>
        /// <returns></returns>
        public static async Task<object?> Get(Type T)
        {
            AssertType(T);

            if (Files.ContainsKey(T))
            {
                while (Files[T]) await Task.Delay(100);
            }
            else return null;

            string filename = Path.Combine(FolderPath, T.Name + ".xml");

            DataContractSerializer serializer = new(T);
            object? result;

            Files[T] = true;

            using (FileStream file = File.OpenRead(filename))
            using (XmlReader reader = XmlReader.Create(file))
            {
                result = serializer.ReadObject(reader);
            }

            Files[T] = false;

            return result;
        }

        /// <summary>
        /// Save a given object to the appdata\local store
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public static async Task Save(object obj)
        {
            Type objType = obj.GetType();

            AssertType(objType);

            if (Files.ContainsKey(objType))
            {
                while (Files[objType]) await Task.Delay(100);
            }
            else Files[objType] = false;

            string filename = Path.Combine(FolderPath, objType.Name + ".xml"); ;

            DataContractSerializer serializer = new(objType);

            Files[objType] = true;

            using (FileStream file = File.Create(filename))
            using (XmlWriter writer = XmlWriter.Create(file))
            {
                serializer.WriteObject(writer, obj);
            }

            Files[objType] = false;
        }
    }
}
