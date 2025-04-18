using OllamaClient.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

namespace OllamaClient.Models
{

    /// <summary>
    /// Generic class for encapsulating a DataContract object to a file. Automatically serializes and deserializes the object to/from XML.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataFile<T>
    {
        private Uri FileUri { get; set; }
        private object FileLock = new();
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
}
