using Microsoft.UI.Xaml.Data;
using OllamaClient.Models.Ollama;
using OllamaClient.LocalStorage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text;

namespace OllamaClient.ViewModels
{
    [DataContract]
    public partial class ChatItem : INotifyPropertyChanged
    {
        private bool ProgRingEnabled { get; set; } = false;

        [DataMember]
        private DateTime? MessageTimestamp { get; set; }
        [DataMember]
        private string ContentString { get; set; } = "";
        [DataMember]
        public string Role { get; set; } = "user";

        public bool ProgressRingEnabled
        {
            get => ProgRingEnabled;
            set
            {
                ProgRingEnabled = value;
                OnPropertyChanged(this, new("ProgressRingEnabled"));
            }
        }

        public DateTime? Timestamp
        {
            get => MessageTimestamp;
            set
            {
                MessageTimestamp = value;
                OnPropertyChanged(this, new("Timestamp"));
            }
        }

        public string Content
        {
            get => ContentString;
            set
            {
                ContentString = value;
                OnPropertyChanged(this, new("Content"));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChatItem(Message message)
        {
            ContentString = message.content ?? "";
            Role = message.role ?? "user";
        }

        public ChatItem(Role role, string content) : this(new Message(role, content)) { }

        protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
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
            get
            {
                return ChatItemCollection;
            }
            set
            {
                ChatItemCollection = value;
                OnPropertyChanged(this, new("Items"));
            }
        }

        public string? Subject
        {
            get => SubjectString;
            set
            {
                SubjectString = value;
                OnPropertyChanged(this, new("Subject"));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StartOfMessage;
        public event EventHandler? EndOfMessasge;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        protected void OnStartOfMessage(object? sender, EventArgs e)
        {
            StartOfMessage?.Invoke(sender, e);
        }

        protected void OnEndOfMessage(object? sender, EventArgs e)
        {
            EndOfMessasge?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        public void Cancel()
        {
            if (CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
            CancellationTokenSource = new();
        }

        public async Task GenerateSubject(Client conn, string prompt)
        {
            if (SelectedModel == null) return;
            if (CancellationTokenSource == null) return;

            CompletionRequest request = new()
            {
                model = "llama3",
                prompt = "Summarize this string, in 4 words or less: " + prompt + ". This string is the opening message in a conversation. Do not include quotation marks.",
                stream = true,
                options = new()
                {
                    num_predict = 10,
                    top_k = 10,
                    top_p = (float?)0.5
                }
            };

            OnStartOfMessage(this, EventArgs.Empty);

            try
            {
                StringBuilder subject = new();

                Progress<CompletionResponse> progress = new((response) => { Subject = subject.Append(response.response).ToString(); });

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<CompletionResponse>? stream = await conn.GenerateCompletionStream(request);

                    if (stream is not null)
                    {
                        using (stream)
                        {
                            await stream.Read(progress, CancellationTokenSource.Token);
                        }
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
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
            finally
            {
                OnEndOfMessage(this, EventArgs.Empty);
            }
        }

        public async Task SendUserMessage(Client conn, string prompt)
        {
            //return early if no model selected
            if (SelectedModel == null) return;

            if (CancellationTokenSource == null) CancellationTokenSource = new();

            ChatItem newChatItem = new(Role.user, prompt);

            newChatItem.Timestamp = DateTime.Now;

            //add user message
            Items.Add(newChatItem);

            //initialize assistant response with empty content
            ChatItem responseChatItem = new(Role.assistant, "");

            responseChatItem.ProgressRingEnabled = true;

            //add assistant message
            Items.Add(responseChatItem);

            //build HTTP request data
            ChatRequest request = new()
            {
                model = SelectedModel,
                messages = Items.Select(m => m.ToMessage()).ToArray(),
                stream = true
            };

            OnStartOfMessage(this, EventArgs.Empty);

            //Make HTTP POST request to ollama with built request, cancel if necessary
            try
            {
                Progress<ChatResponse> progress = new (s => responseChatItem.Content = responseChatItem.Content + s.message?.content ?? "");

                await Task.Run(async () =>
                {
                    DelimitedJsonStream<ChatResponse>? stream = await conn.ChatStream(request);

                    //Send HTTP request and read JSON stream, passing parsed objects to responseChatItem via an IProgress<ChatResponse> interface
                    if (stream is not null)
                    {
                        using (stream)
                        {
                            await stream.Read(progress, CancellationTokenSource.Token).ConfigureAwait(false);
                        }
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
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
            finally
            {
                responseChatItem.ProgressRingEnabled = false;
                responseChatItem.Timestamp = DateTime.Now;
                OnEndOfMessage(this, EventArgs.Empty);
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
                OnPropertyChanged(this, new("Items"));
            }
        }

        public event EventHandler? ModelTagsLoaded;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnModelTagsLoaded(object? sender, EventArgs e)
        {
            ModelTagsLoaded?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private async void Conversation_StartOfMessage(object? sender, EventArgs e)
        {
            await Save();
        }

        private async void Conversation_EndOfMessage(object? sender, EventArgs e)
        {
            await Save();
        }

        public void Create()
        {
            Conversation newConv = new();

            newConv.StartOfMessage += Conversation_StartOfMessage;
            newConv.EndOfMessasge += Conversation_EndOfMessage;

            Items.Add(newConv);
        }

        public async Task LoadAvailableModels(Client conn)
        {
            try
            {
                List<string> results = [];

                await Task.Run(async () =>
                {
                    if (await conn.ListModels() is ListModelsResponse response && response.models is ModelInfo[] models)
                    {
                        foreach (ModelInfo model in models)
                        {
                            if (model.model != null) results.Add(model.model);
                        }
                    }
                });

                foreach (string s in results)
                {
                    AvailableModels.Add(s);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
            finally
            {
                OnModelTagsLoaded(this, EventArgs.Empty);
            }
        }

        public async Task LoadSavedConversations()
        {
            try
            {
                ObservableCollection<Conversation>? result = null;

                await Task.Run(async () =>
                {
                    if (await Persistence.Get(typeof(Conversations)) is Conversations savedConvos && savedConvos.Items != null)
                    {
                        result = savedConvos.Items;
                    }
                });

                if (result != null)
                {
                    foreach (Conversation c in result)
                    {
                        c.StartOfMessage += Conversation_StartOfMessage;
                        c.EndOfMessasge += Conversation_EndOfMessage;
                        Items.Add(c);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }
            catch (XmlException)
            {
                return;
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
        }

        public async Task Save()
        {
            try
            {
                await Task.Run(async () =>
                {
                    await Persistence.Save(this);
                });
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(ex, false));
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
