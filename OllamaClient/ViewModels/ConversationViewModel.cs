using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    public partial class ConversationViewModel : INotifyPropertyChanged
    {
        public class Settings
        {
            public CompletionRequest SubjectRequest { get; set; }
        }

        private readonly ILogger _Logger;
        private readonly Settings _Settings;
        private readonly OllamaApiService _Api;
        private CancellationTokenSource _CancellationTokenSource { get; set; } = new();
        private ObservableCollection<ChatMessageViewModel> _ChatMessages { get; set; } = [];
        private string? _Subject { get; set; }
        private string? _SelectedModel { get; set; }

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _ChatMessages;
            set
            {
                _ChatMessages = value;
                OnPropertyChanged();
            }
        }

        public string Subject
        {
            get => _Subject ?? "New Conversation";
            set
            {
                _Subject = value;
                OnPropertyChanged();
            }
        }

        public string? SelectedModel
        {
            get => _SelectedModel;
            set
            {
                _SelectedModel = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StartOfRequest;
        public event EventHandler? EndOfResponse;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        public ConversationViewModel(ILogger<ConversationViewModel> logger, IOptions<Settings> options)
        {
            _Logger = logger;
            _Settings = options.Value;

            if (App.GetService<OllamaApiService>() is OllamaApiService api)
            {
                _Api = api;
            }
            else throw new ArgumentNullException(nameof(api));
        }

        protected void OnStartOfRequest(EventArgs e)
        {
            
            StartOfRequest?.Invoke(this, e);
        }

        protected void OnEndOfResponse(EventArgs e)
        {
            EndOfResponse?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public void SetConversation(Conversation conversation)
        {
            _ChatMessages.Clear();

            _Subject = conversation.Subject;
            _SelectedModel = conversation.SelectedModel;
            foreach (ChatMessage chatMessage in conversation.ChatMessageCollection)
            {
                _ChatMessages.Add(new(chatMessage, false));
            }
        }

        public Conversation GetConversation()
        {
            Conversation conversation = new();
            conversation.Subject = _Subject;
            conversation.SelectedModel = _SelectedModel;
            foreach (var viewModel in _ChatMessages)
            {
                if (viewModel.GetChatMessage() is ChatMessage chatMessage)
                {
                    conversation.ChatMessageCollection.Add(chatMessage);
                }
            }

            return conversation;
        }

        public void Cancel()
        {
            if (_CancellationTokenSource is not null)
            {
                _CancellationTokenSource.Cancel();
            }
            _CancellationTokenSource = new();
        }

        public async Task GenerateSubject(string prompt)
        {
            CompletionRequest request = _Settings.SubjectRequest;

            if (request.prompt.Contains("$Prompt"))
            {
                request.prompt = request.prompt.Replace("$Prompt", prompt);
            }

            OnStartOfRequest(EventArgs.Empty);

            try
            {
                StringBuilder subject = new();

                Progress<CompletionResponse> progress = new(r => subject.Append(r.response));

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<CompletionResponse> stream = await _Api.CompletionStream(request);

                    using (stream)
                    {
                        await stream.Read(progress, _CancellationTokenSource.Token);
                    }
                });

                
                Subject = subject.ToString();
                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' successful", _SelectedModel);
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' cancelled", _SelectedModel);
            }
            catch (Exception e)
            {
                _Logger.LogError("Subject generation for conversation with '{SelectedModel}' failed", _SelectedModel, e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task SendUserMessage(string prompt)
        {
            if (_CancellationTokenSource == null) _CancellationTokenSource = new();

            //return early if no model selected
            if (_SelectedModel == null) return;


            //add user message
            ChatMessageViewModel userChatMessage = new(new(Role.user, prompt, DateTime.Now), false);
            _ChatMessages.Add(userChatMessage);


            //add assistant message
            ChatMessageViewModel assistantChatMessage = new(new(Role.assistant, ""), true);
            _ChatMessages.Add(assistantChatMessage);


            //build HTTP request data
            ChatRequest request = new()
            {
                model = _SelectedModel,
                messages = _ChatMessages.Select(m => m.GetChatMessage()?.ToMessage() ?? new()).ToArray(),
                stream = true
            };

            OnStartOfRequest(EventArgs.Empty);

            //Make HTTP POST request to ollama with built request, cancel if necessary
            try
            {
                StringBuilder content = new();

                Progress<ChatResponse> progress = new(s => assistantChatMessage.Content = content.Append(s.message?.content ?? "").ToString());

                await Task.Run(async () =>
                {
                    //Send HTTP request and read JSON stream, passing parsed objects to responseChatItem via an IProgress<ChatResponse> interface
                    using DelimitedJsonStream<ChatResponse> stream = await _Api.ChatStream(request);
                    await stream.Read(progress, _CancellationTokenSource.Token).ConfigureAwait(false);
                });

                _Logger.LogInformation("Chat completion for conversation with '{ConversationSelectedModel}' successful", _SelectedModel);
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Chat completion for conversation with '{ConversationSelectedModel}' cancelled", _SelectedModel);
            }
            catch (Exception e)
            {
                _Logger.LogError("Chat completion for conversation with '{ConversationSelectedModel}' failed", _SelectedModel, e);
                OnUnhandledException(new(e, false));
            }
            finally
            {
                assistantChatMessage.ProgressRingEnabled = false;
                assistantChatMessage.SetTimestamp(DateTime.Now);
                OnEndOfResponse(EventArgs.Empty);
            }
        }
    }
}
