using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OllamaClient.Models;
using OllamaClient.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{
    [KnownType(typeof(ChatMessageViewModel))]
    [DataContract]
    public partial class ConversationViewModel : INotifyPropertyChanged
    {
        public class Settings(CompletionRequest subjectRequest)
        {
            public CompletionRequest SubjectRequest { get; set; } = subjectRequest;
        }

        private readonly ILogger _Logger;
        private readonly Settings _Settings;
        private readonly OllamaApiService _Api;
        private CancellationTokenSource _CancellationTokenSource { get; set; } = new();

        [DataMember]
        private ObservableCollection<ChatMessageViewModel> _ChatMessageCollection { get; set; } = [];
        [DataMember]
        private string? _Subject { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }

        public ObservableCollection<ChatMessageViewModel> Items
        {
            get => _ChatMessageCollection;
            set
            {
                _ChatMessageCollection = value;
                OnPropertyChanged();
            }
        }

        public string? Subject
        {
            get => _Subject;
            set
            {
                _Subject = value;
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

            if(App.GetService<OllamaApiService>() is OllamaApiService api)
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

        public void Cancel()
        {
            if(_CancellationTokenSource is not null)
            {
                _CancellationTokenSource.Cancel();
            }
            _CancellationTokenSource = new();
        }

        public async Task GenerateSubject(string prompt)
        {
            if (SelectedModel == null) return;
            if (Subject is null) Subject = "";

            CompletionRequest request = _Settings.SubjectRequest;

            if (request.prompt.Contains("$Prompt$"))
            {
                request.prompt = request.prompt.Replace("$Prompt$", prompt);
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
                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' successful", SelectedModel);
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Subject generation for conversation with '{SelectedModel}' cancelled", SelectedModel);
            }
            catch (Exception e)
            {
                _Logger.LogError("Subject generation for conversation with '{SelectedModel}' failed", SelectedModel, e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task SendUserMessage(string prompt)
        {
            if (_CancellationTokenSource == null) _CancellationTokenSource = new();

            //return early if no model selected
            if (SelectedModel == null) return;

            ChatMessageViewModel userChatMessage = new()
            {
                Content = prompt
            };

            //add user message
            Items.Add(userChatMessage);

            //initialize assistant response with empty content
            ChatMessageViewModel assistantChatMessage = new()
            {
                Role = "assistant",
                ProgressRingEnabled = true
            };

            //add assistant message
            Items.Add(assistantChatMessage);

            //build HTTP request data
            ChatRequest request = new()
            {
                model = SelectedModel,
                messages = Items.Select(m => m.ToMessage()).ToArray(),
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

                _Logger.LogInformation("Chat completion for conversation with '{SelectedModel}' successful", SelectedModel);
            }
            catch (TaskCanceledException)
            {
                _Logger.LogInformation("Chat completion for conversation with '{SelectedModel}' cancelled", SelectedModel);
            }
            catch (Exception e)
            {
                _Logger.LogError("Chat completion for conversation with '{SelectedModel}' failed", SelectedModel, e);
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
