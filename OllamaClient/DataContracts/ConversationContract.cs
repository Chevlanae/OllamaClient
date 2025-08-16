using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
