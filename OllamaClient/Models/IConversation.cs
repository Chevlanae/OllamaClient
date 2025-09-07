using OllamaClient.Services.DataContracts;
using OllamaClient.Services.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public interface IConversation
    {
        List<IChatMessage> ChatMessageCollection { get; set; }
        string? SelectedModel { get; set; }
        string? Subject { get; set; }

        event EventHandler<Conversation.EventArgs.EndOfChatRequest>? EndOfChatRequest;
        event EventHandler<Conversation.EventArgs.EndOfCompletionRequest>? EndOfCompletionRequest;
        event EventHandler<Conversation.EventArgs.StartOfChatRequest>? StartOfChatRequest;
        event EventHandler<Conversation.EventArgs.StartOfCompletionRequest>? StartOfCompletionRequest;
        event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        IEnumerable<ChatMessage> BuildUserChatMessageAndResponse(string prompt);
        void Cancel();
        void CopyContract(ConversationContract contract);
        Task<bool> GenerateSubject(string prompt, List<string> models, IProgress<CompletionResponse> progress);
        Task<bool> SendChatRequest(IProgress<ChatResponse> progress);
        void SetSystemMessage(string systemMessage);
        ConversationContract ToSerializable();
    }
}