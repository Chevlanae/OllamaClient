using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace OllamaClient
{
    internal static class LocalStorage
    {
        private static readonly Microsoft.Windows.Storage.ApplicationData AppData = Microsoft.Windows.Storage.ApplicationData.GetDefault();

        public static bool IsSaving = false;

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

            string filename = T.Name + ".xml";

            StorageFile classFile = await AppData.LocalFolder.GetFileAsync(filename);
            DataContractSerializer serializer = new(T);

            using (Stream stream = await classFile.OpenStreamForReadAsync())
            {
                return serializer.ReadObject(stream);
            }
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

            while (IsSaving) await Task.Delay(100);

            IsSaving = true;
            string filename = objType.Name + ".xml";

            StorageFile classFile = await AppData.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            DataContractSerializer serializer = new(objType);

            using (Stream stream = await classFile.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(stream, obj);
            }

            IsSaving = false;
        }
    }
}
