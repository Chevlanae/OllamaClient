using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    [DataContract]
    public class ChatMessage(Role role, string content, DateTime? timestamp = null)
    {
        [DataMember]
        public Role Role { get; set; } = role;
        [DataMember]
        public string Content { get; set; } = content;
        [DataMember]
        public DateTime? Timestamp { get; set; } = timestamp;


        public Message ToMessage()
        {
            return new Message()
            {
                role = Role.ToString(),
                content = Content
            };
        }
    }
}
