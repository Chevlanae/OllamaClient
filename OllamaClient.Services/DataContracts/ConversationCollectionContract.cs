using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace OllamaClient.Services.DataContracts
{
    [KnownType(typeof(ConversationContract))]
    [KnownType(typeof(ChatMessageContract))]
    [DataContract]
    public class ConversationCollectionContract
    {
        [DataMember]
        public required List<ConversationContract> Items { get; set; }
        [DataMember]
        public required List<string> AvailableModels { get; set; }
        [DataMember]
        public DateTime? LastUpdated { get; set; }
    }
}
