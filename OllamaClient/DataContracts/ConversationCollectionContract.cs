using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.DataContracts
{
    [KnownType(typeof(ConversationContract))]
    [KnownType(typeof(ChatMessageContract))]
    [DataContract]
    public class ConversationCollectionContract
    {
        [DataMember]
        public List<ConversationContract> Items { get; set; }
        [DataMember]
        public List<string> AvailableModels { get; set; }
        [DataMember]
        public DateTime? LastUpdated { get; set; }
    }
}
