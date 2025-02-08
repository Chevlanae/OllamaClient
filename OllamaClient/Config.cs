using Microsoft.Windows.Storage;
using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient
{
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

    internal static class Config
    {
        private static readonly ApplicationData AppData = ApplicationData.GetDefault();

        public static bool IsSavingData = false;

        public static async Task SaveConversations(Conversations c)
        {
            AppState state = new(c);

            IsSavingData = true;

            Windows.Storage.StorageFile appstateFile = await AppData.LocalFolder.CreateFileAsync("appstate.xml", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            DataContractSerializer serializer = new(typeof(AppState));
            using (Stream stream = await appstateFile.OpenStreamForWriteAsync())
            {
                serializer.WriteObject(stream, state);
            }

            IsSavingData = false;
        }

        public static async Task<AppState> GetSavedAppState()
        {
            Windows.Storage.StorageFile appstateFile = await AppData.LocalFolder.GetFileAsync("appstate.xml");
            DataContractSerializer serializer = new(typeof(AppState));
            using (Stream stream = await appstateFile.OpenStreamForReadAsync())
            {
                if (serializer.ReadObject(stream) is AppState state) return state;
                else throw new ApplicationException("Could not read AppState file at " + appstateFile.Path);
            }
        }
    }
}
