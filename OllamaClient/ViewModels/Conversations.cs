using Microsoft.UI.Xaml.Data;
using OllamaClient.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using OllamaClient.Services;

namespace OllamaClient.ViewModels
{
    [DataContract]
    public partial class ChatItem : INotifyPropertyChanged
    {
        private bool RingEnabled { get; set; } = false;

        [DataMember]
        private DateTime? MessageTimestamp { get; set; }
        [DataMember]
        private string ContentString { get; set; } = "";
        [DataMember]
        public string Role { get; set; } = "user";

        public bool ProgressRingEnabled
        {
            get => RingEnabled;
            set
            {
                RingEnabled = value;
                OnPropertyChanged();
            }
        }

        public DateTime? Timestamp
        {
            get => MessageTimestamp;
            set
            {
                MessageTimestamp = value;
                OnPropertyChanged();
            }
        }

        public string Content
        {
            get => ContentString;
            set
            {
                ContentString = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public Message ToMessage()
        {
            return new Message()
            {
                role = Role,
                content = Content
            };
        }
    }

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
            if(Subject is null) Subject = "";

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
            catch (TaskCanceledException e)
            {
                Debug.Write(e);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new (e, false));
            }
            finally
            {
                OnEndOfResponse(EventArgs.Empty);
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
            catch (TaskCanceledException e)
            {
                Debug.Write(e);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new (e, false));
            }
            finally
            {
                responseChatItem.ProgressRingEnabled = false;
                responseChatItem.Timestamp = DateTime.Now;
                OnEndOfResponse(EventArgs.Empty);
            }
        }
    }

    [KnownType(typeof(ChatItem))]
    [KnownType(typeof(Conversation))]
    [DataContract]
    public class Conversations : INotifyPropertyChanged
    {
        [DataMember]
        private ObservableCollection<Conversation> ConversationCollection { get; set; } = [];

        public ObservableCollection<string> AvailableModels { get; set; } = [];

        public ObservableCollection<Conversation> Items
        {
            get => ConversationCollection;
            set
            {
                ConversationCollection = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler? ModelsLoaded;
        public event EventHandler? ModelsLoadFailed;
        public event EventHandler? ConversationsLoaded;
        public event EventHandler? ConversationsLoadFailed;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnModelsLoaded(EventArgs e)
        {
            ModelsLoaded?.Invoke(this, e);
        }

        protected void OnModelsLoadFailed(EventArgs e)
        {
            ModelsLoadFailed?.Invoke(this, e);
        }

        protected void OnConversationsLoaded(EventArgs e)
        {
            ConversationsLoaded?.Invoke(this, e);
        }

        protected void OnConversationsLoadFailed(EventArgs e)
        {
            ConversationsLoadFailed?.Invoke(this, e);
        }

        protected void OnUnhandledException(UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(this, e);
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public async Task LoadModels()
        {
            try
            {
                string[] results = await Task.Run(async () =>
                {
                    ListModelsResponse response = await Api.ListModels();

                    return response.models.Select(m => m.model).ToArray();
                });

                AvailableModels = [.. results];

                OnModelsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new (e, false));
                OnModelsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task LoadConversations()
        {
            try
            {
                Items.Clear();

                if(await Task.Run(DataFileService.Get<Conversations>) is Conversations result)
                {
                    foreach (Conversation c in result.Items)
                    {
                        Items.Add(c);
                    }
                }

                OnConversationsLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new (e, false));
                OnConversationsLoadFailed(EventArgs.Empty);
            }
        }

        public async Task Save()
        {
            try
            {
                await Task.Run(() => { DataFileService.Set(this); });
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(new(e, false));
            }
        }
    }

    public partial class ChatItemTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime timestamp)
            {
                return timestamp.ToLocalTime().ToShortDateString() + " " + timestamp.ToLocalTime().ToShortTimeString();
            }
            else return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemTextBoxBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChatItem item && item.Role == Enum.GetName(Role.assistant))
            {
                return "Transparent";
            }
            else return "DimGray";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChatItem item && item.Role == Enum.GetName(Role.assistant))
            {
                return "Left";
            }
            else return "Right";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemProgressRingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool v && v)
            {
                return "Visible";
            }
            else return "Collapsed";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemSubjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return "New conversation";
            }
            else return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
