using OllamaClient.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;

namespace OllamaClient
{

    /// <summary>
    /// Serializable class for saving and loading the AppState object
    /// </summary>
    [DataContract]
    [KnownType(typeof(ChatItemSerializable))]
    [KnownType(typeof(ConversationSerializable))]
    public class AppState
    {
        [DataMember]
        public ConversationSerializable[] Conversations { get; set; }

        public AppState(Conversations conversations)
        {
            Conversations = conversations.Select(c => new ConversationSerializable(c)).ToArray();
        }
    }

    /// <summary>
    /// Local storage class for saving and loading app related data
    /// </summary>

    internal static class LocalStorage
    {
        private static readonly Microsoft.Windows.Storage.ApplicationData AppData = Microsoft.Windows.Storage.ApplicationData.GetDefault();

        public static bool IsSavingData = false;


        /// <summary>
        /// Save the current state of the Conversations object to the app's local storage
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static async Task SaveConversations(Conversations c)
        {
            AppState state = new(c);

            IsSavingData = true;

            StorageFile appstateFile = await AppData.LocalFolder.CreateFileAsync("appstate.xml", CreationCollisionOption.ReplaceExisting);
            DataContractSerializer serializer = new(typeof(AppState));
            using (Stream stream = await appstateFile.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(stream, state);
            }

            IsSavingData = false;
        }

        /// <summary>
        /// Load the saved AppState object from the app's local storage
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public static async Task<AppState> GetSavedAppState()
        {
            StorageFile appstateFile = await AppData.LocalFolder.GetFileAsync("appstate.xml");
            DataContractSerializer serializer = new(typeof(AppState));
            using (Stream stream = await appstateFile.OpenStreamForReadAsync())
            {
                if (serializer.ReadObject(stream) is AppState state) return state;
                else throw new ApplicationException("Could not read AppState file at " + appstateFile.Path);
            }
        }
    }
}
