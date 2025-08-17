using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.DataContracts
{
    [KnownType(typeof(ChatMessageContract))]
    [DataContract]
    public class ConversationContract
    {
        [DataMember]
        public List<ChatMessageContract> ChatMessageCollection { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }
        [DataMember]
        public string? Subject { get; set; }
    }
}
