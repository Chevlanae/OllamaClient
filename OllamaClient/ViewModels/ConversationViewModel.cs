using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.Json;
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
        private ObservableCollection<ChatMessageViewModel> _ChatMessageViewModels { get; set; } = [];
        private string? _Subject { get; set; }
        private string? _SelectedModel { get; set; }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StartOfRequest;
        public event EventHandler? EndOfResponse;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnStartOfRequest(EventArgs e) => StartOfRequest?.Invoke(this, e);
        protected void OnEndOfResponse(EventArgs e) => EndOfResponse?.Invoke(this, e);
        protected void OnUnhandledException(UnhandledExceptionEventArgs e) => UnhandledException?.Invoke(this, e);
        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _ChatMessageViewModels;
            set
            {
                _ChatMessageViewModels = value;
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

        public void CopyFromConversation(Conversation conversation)
        {
            _ChatMessageViewModels.Clear();

            _Subject = conversation.Subject;
            _SelectedModel = conversation.SelectedModel;

            foreach (ChatMessage chatMessage in conversation.ChatMessageCollection)
            {
                _ChatMessageViewModels.Add(new(chatMessage.Role, chatMessage.Content, chatMessage.Timestamp));
            }
        }

        public Conversation ToConversation() => new()
        {
            Subject = _Subject,
            SelectedModel = _SelectedModel,
            ChatMessageCollection = _ChatMessageViewModels.Select(vm => vm.ToChatMessage()).ToList()
        };

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
                _Logger.LogError(e, "Subject generation for conversation with '{SelectedModel}' failed", _SelectedModel);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task SendUserMessage(string prompt)
        {
            //return early if no model selected
            if (_SelectedModel == null) return;

            if (_CancellationTokenSource == null) _CancellationTokenSource = new();

            //add user message
            ChatMessageViewModel userChatMessage = new(Role.user, prompt);
            _ChatMessageViewModels.Add(userChatMessage);


            //add assistant message
            ChatMessageViewModel assistantChatMessage = new(Role.assistant, "", progressRingEnabled: true);
            _ChatMessageViewModels.Add(assistantChatMessage);


            //build HTTP request data
            ChatRequest request = new()
            {
                model = _SelectedModel,
                messages = _ChatMessageViewModels.Select(m => m.ToMessage()).ToArray(),
            };

            OnStartOfRequest(EventArgs.Empty);

            try
            {
                StringBuilder content = new();

                Progress<ChatResponse> progress = new(s => assistantChatMessage.Content = content.Append(s.message?.content ?? "").ToString());

                await Task.Run(async () =>
                {
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
                _Logger.LogError(e, "Chat completion for conversation with '{ConversationSelectedModel}' failed", _SelectedModel);
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
