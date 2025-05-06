using Microsoft.Extensions.Logging;
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
    [KnownType(typeof(ChatMessage))]
    [DataContract]
    public partial class Conversation : INotifyPropertyChanged
    {
        private CancellationTokenSource? _CancellationTokenSource { get; set; }

        [DataMember]
        private ObservableCollection<ChatMessage> _ChatMessageCollection { get; set; } = [];
        [DataMember]
        private string? _Subject { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }

        public ObservableCollection<ChatMessage> Items
        {
            get => _ChatMessageCollection;
            set
            {
                _ChatMessageCollection = value;
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

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StartOfRequest;
        public event EventHandler? EndOfResponse;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

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
            if (_CancellationTokenSource != null)
            {
                _CancellationTokenSource.Cancel();
            }
            _CancellationTokenSource = new();
        }

        public async Task GenerateSubject(string prompt)
        {
            if (SelectedModel == null) return;
            if (_CancellationTokenSource == null) _CancellationTokenSource = new();
            if (Subject is null) Subject = "";

            CompletionRequest request = Settings.SubjectGenerationOptions;

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
                    DelimitedJsonStream<CompletionResponse> stream = await Api.CompletionStream(request);

                    using (stream)
                    {
                        await stream.Read(progress, _CancellationTokenSource.Token);
                    }
                });

                Subject = subject.ToString();
                Logging.Log($"Subject generation for conversation with '{SelectedModel}' successful", LogLevel.Information);
            }
            catch (TaskCanceledException)
            {
                Logging.Log($"Subject generation for conversation with '{SelectedModel}' cancelled", LogLevel.Information);
            }
            catch (Exception e)
            {
                Logging.Log($"Subject generation for conversation with '{SelectedModel}' failed", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task SendUserMessage(string prompt)
        {
            //return early if no model selected
            if (SelectedModel == null) return;

            if (_CancellationTokenSource == null) _CancellationTokenSource = new();

            ChatMessage newChatItem = new()
            {
                Content = prompt
            };

            newChatItem.SetTimestamp(DateTime.Now);

            //add user message
            Items.Add(newChatItem);

            //initialize assistant response with empty content
            ChatMessage responseChatItem = new()
            {
                Role = "assistant",
                ProgressRingEnabled = true
            };

            //add assistant message
            Items.Add(responseChatItem);

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

                Progress<ChatResponse> progress = new(s => responseChatItem.Content = content.Append(s.message?.content ?? "").ToString());

                await Task.Run(async () =>
                {
                    //Send HTTP request and read JSON stream, passing parsed objects to responseChatItem via an IProgress<ChatResponse> interface
                    DelimitedJsonStream<ChatResponse> stream = await Api.ChatStream(request);
                    using (stream)
                    {
                        await stream.Read(progress, _CancellationTokenSource.Token).ConfigureAwait(false);
                    }
                });

                Logging.Log($"Chat completion for conversation with '{SelectedModel}' successful", LogLevel.Information);
            }
            catch (TaskCanceledException)
            {
                Logging.Log($"Chat completion for conversation with '{SelectedModel}' cancelled", LogLevel.Information);
            }
            catch (Exception e)
            {
                Logging.Log($"Chat completion for conversation with '{SelectedModel}' failed", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
            }
            finally
            {
                responseChatItem.ProgressRingEnabled = false;
                responseChatItem.SetTimestamp(DateTime.Now);
                OnEndOfResponse(EventArgs.Empty);
            }
        }
    }
}
