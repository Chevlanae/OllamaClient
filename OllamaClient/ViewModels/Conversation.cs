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
    [KnownType(typeof(ChatItem))]
    [DataContract]
    public partial class Conversation : INotifyPropertyChanged
    {
        private CancellationTokenSource? CancellationTokenSource { get; set; }

        [DataMember]
        private ObservableCollection<ChatItem> ChatItemCollection { get; set; } = [];
        [DataMember]
        private string? SubjectString { get; set; }
        [DataMember]
        public string? SelectedModel { get; set; }
        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public ObservableCollection<ChatItem> Items
        {
            get => ChatItemCollection;
            set
            {
                ChatItemCollection = value;
                OnPropertyChanged();
            }
        }

        public string? Subject
        {
            get => SubjectString;
            set
            {
                SubjectString = value;
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
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource = new();
        }

        public async Task GenerateSubject(string prompt)
        {
            if (SelectedModel == null) return;
            if (CancellationTokenSource == null) CancellationTokenSource = new();
            if (Subject is null) Subject = "";

            CompletionRequest request = new()
            {
                model = "llama3:8b",
                prompt = "Summarize this string, in 4 words or less: " + prompt + ". This string is the opening message in a conversation. Do not include quotation marks.",
                stream = true,
                options = new()
                {
                    num_predict = 10,
                    top_k = 10,
                    top_p = (float?)0.5
                }
            };

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
                        await stream.Read(progress, CancellationTokenSource.Token);
                    }
                });

                Subject = subject.ToString();
            }
            catch (TaskCanceledException)
            {
                Logging.Log($"Subject generation cancelled for conversation with {SelectedModel}", LogLevel.Information);
            }
            catch (Exception e)
            {
                Logging.Log($"Subject generation failed for conversation with {SelectedModel}", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
            }
        }

        public async Task SendUserMessage(string prompt)
        {
            //return early if no model selected
            if (SelectedModel == null) return;

            if (CancellationTokenSource == null) CancellationTokenSource = new();

            ChatItem newChatItem = new()
            {
                Content = prompt,
                Timestamp = DateTime.Now
            };

            //add user message
            Items.Add(newChatItem);

            //initialize assistant response with empty content
            ChatItem responseChatItem = new()
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
                        await stream.Read(progress, CancellationTokenSource.Token).ConfigureAwait(false);
                    }
                });
            }
            catch (TaskCanceledException)
            {
                Logging.Log($"Chat completion cancelled for conversation with '{SelectedModel}'", LogLevel.Information);
            }
            catch (Exception e)
            {
                Logging.Log($"Chat completion failed for conversation with '{SelectedModel}'", LogLevel.Error, e);
                OnUnhandledException(new(e, false));
            }
            finally
            {
                responseChatItem.ProgressRingEnabled = false;
                responseChatItem.Timestamp = DateTime.Now;
                OnEndOfResponse(EventArgs.Empty);
            }
        }
    }
}
