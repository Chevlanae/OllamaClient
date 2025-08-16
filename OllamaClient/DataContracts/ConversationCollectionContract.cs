using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
