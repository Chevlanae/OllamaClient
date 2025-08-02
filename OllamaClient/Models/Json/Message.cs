using System;

namespace OllamaClient.Models.Json
{
    public record struct Message : IMessage
    {
        public string? role { get; set; }
        public string? content { get; set; }

        public Message(Role role, string content)
        {
            this.role = Enum.GetName(role);
            this.content = content;
        }
    }
}
