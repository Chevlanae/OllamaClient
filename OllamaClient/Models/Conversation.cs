using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.DataContracts;
using OllamaClient.Json;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.Models
{
    public class Conversation : IConversation
    {
        public class Settings
        {
            public CompletionRequest SubjectRequest { get; set; }
        }

        public class EventArgs
        {
            public class StartOfCompletionRequest : System.EventArgs
            {
                public CompletionRequest Request { get; set; }
            }

            public class EndOfCompletionRequest : System.EventArgs
            {
            }

            public class StartOfChatRequest : System.EventArgs
            {
                public ChatRequest Request { get; set; }
            }

            public class EndOfChatRequest : System.EventArgs 
            { 
            }
        }

        public List<IChatMessage> ChatMessageCollection { get; set; } = [];
        public string? Subject { get; set; }
        public string? SelectedModel { get; set; }

        private readonly ILogger _Logger;
        private readonly Settings _Settings;
        private readonly IOllamaApiService _Api;
        private CancellationTokenSource _CancellationTokenSource = new();

        public Conversation(ILogger<Conversation> logger, IOptions<Settings> options, IOllamaApiService api)
        {
            _Logger = logger;
            _Settings = options.Value;
            _Api = api;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<EventArgs.StartOfCompletionRequest>? StartOfCompletionRequest;
        public event EventHandler<EventArgs.EndOfCompletionRequest>? EndOfCompletionRequest;
        public event EventHandler<EventArgs.StartOfChatRequest>? StartOfChatRequest;
        public event EventHandler<EventArgs.EndOfChatRequest>? EndOfChatRequest;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnStartOfCompletionRequest(EventArgs.StartOfCompletionRequest e) => StartOfCompletionRequest?.Invoke(this, e);
        protected void OnEndOfCompletionRequest(EventArgs.EndOfCompletionRequest e) => EndOfCompletionRequest?.Invoke(this, e);
        protected void OnStartOfChatRequest(EventArgs.StartOfChatRequest e) => StartOfChatRequest?.Invoke(this, e);
        protected void OnEndOfChatRequest(EventArgs.EndOfChatRequest e) => EndOfChatRequest?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

        public void CopyContract(ConversationContract contract)
        {
            ChatMessageCollection = contract.ChatMessageCollection.Select(c =>
            {
                ChatMessage newChatMessage = new(c.Role, c.Content, c.Timestamp);
                return (IChatMessage)newChatMessage;
            }).ToList();

            Subject = contract.Subject;
            SelectedModel = contract.SelectedModel;
        }

        public ConversationContract ToSerializable()
        {
            return new ()
            {
                ChatMessageCollection = ChatMessageCollection.Select(c => c.ToSerializable()).ToList(),
                Subject = Subject,
                SelectedModel = SelectedModel
            };
        }

        public void Cancel()
        {
            if (_CancellationTokenSource is not null)
            {
                _CancellationTokenSource.Cancel();
            }
            _CancellationTokenSource = new();
        }

        public async Task<bool> GenerateSubject(string prompt, IProgress<CompletionResponse> progress)
        {
            CompletionRequest request = _Settings.SubjectRequest;

            if (request.prompt.Contains("$Prompt"))
            {
                request.prompt = request.prompt.Replace("$Prompt", prompt);
            }

            OnStartOfCompletionRequest(new() { Request = request });

            try
            {
                await Task.Run(async () =>
                {
                    DelimitedJsonStream<CompletionResponse> stream = await _Api.CompletionStream(request);

                    using (stream)
                    {
                        await stream.Read(progress, _CancellationTokenSource.Token);
                    }
                });

                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' successful", SelectedModel);
                OnEndOfCompletionRequest(new());
                return true;
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' cancelled", SelectedModel);
                return false;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Subject generation for conversation with '{SelectedModel}' failed", SelectedModel);
                OnUnhandledException(new(e, false));
                return false;
            }
        }

        /// <summary>
        /// Build user chat message by initializing a user message, and then the assistant response message.
        /// Returns the user chat message in a tuple with with assistant chat message.
        /// The tuple is ordered &ltUserChatMessage, AssistantChatMessage&gt.
        /// Call the 'SendChatRequest' method to send the built messages.
        /// </summary>
        /// <param name="prompt">The prompt for the user chat messsage</param>
        /// <returns>A tuple with the user chat message, and the empty assistant response message</returns>
        public Tuple<ChatMessage, ChatMessage> BuildUserChatMessage(string prompt)
        {
            ChatMessage userChatMessage = new(Role.user, prompt);
            ChatMessageCollection.Add(userChatMessage);
            ChatMessage assistantChatMessage = new(Role.assistant, "");
            ChatMessageCollection.Add(assistantChatMessage);
            return new(userChatMessage, assistantChatMessage);
        }

        public async Task<bool> SendChatRequest(IProgress<ChatResponse> progress)
        {
            //return early if no model selected
            if (SelectedModel is null) return false;

            if (_CancellationTokenSource == null) _CancellationTokenSource = new();

            //build HTTP request data
            ChatRequest request = new()
            {
                model = SelectedModel,
                messages = ChatMessageCollection.Select(m => m.ToMessage()).ToArray(),
            };

            OnStartOfChatRequest(new() { Request = request });

            try
            {
                await Task.Run(async () =>
                {
                    using DelimitedJsonStream<ChatResponse> stream = await _Api.ChatStream(request);
                    await stream.Read(progress, _CancellationTokenSource.Token).ConfigureAwait(false);
                });

                _Logger.LogInformation("Chat completion for conversation with '{ConversationSelectedModel}' successful", SelectedModel);
                OnEndOfChatRequest(new());
                return true;
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Chat completion for conversation with '{ConversationSelectedModel}' cancelled", SelectedModel);
                return false;
            }
            catch (Exception e)
            {
                _Logger.LogError(e, "Chat completion for conversation with '{ConversationSelectedModel}' failed", SelectedModel);
                OnUnhandledException(new(e, false));
                return false;
            }
        }
    }
}
