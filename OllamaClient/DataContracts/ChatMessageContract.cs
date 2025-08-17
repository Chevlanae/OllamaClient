using OllamaClient.Json;
using System;
using System.Runtime.Serialization;

namespace OllamaClient.DataContracts
{
    [DataContract]
    public class ChatMessageContract
    {
        [DataMember]
        public Role Role { get; set; }
        [DataMember]
        public string Content { get; set; }
        [DataMember]
        public DateTime? Timestamp { get; set; }
    }
}
