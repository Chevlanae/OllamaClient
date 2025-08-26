using OllamaClient.Services.Json;
using System;
using System.Runtime.Serialization;

namespace OllamaClient.Services.DataContracts
{
    [DataContract]
    public class ChatMessageContract
    {
        [DataMember]
        public required Role Role { get; set; }
        [DataMember]
        public required string Content { get; set; }
        [DataMember]
        public DateTime? Timestamp { get; set; }
    }
}
