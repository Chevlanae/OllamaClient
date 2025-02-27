using Microsoft.UI.Xaml.Data;
using OllamaClient.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OllamaClient.ViewModels
{

    public partial class ChatItem : INotifyPropertyChanged
    {
        private string ContentString { get; set; }

        public string Role { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Content
        {
            get => ContentString;
            set
            {
                ContentString = value;
                OnPropertyChanged();
            }
        }

        public IProgress<ChatResponse> Progress { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ChatItem(Role role, string content) : this(new Message(role, content)) { }

        public ChatItem(Message message)
        {
            ContentString = message.content ?? "";
            Role = message.role ?? "user";
            Timestamp = DateTime.Now;
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }

        public ChatItem(ChatItemSerializable serializable)
        {
            ContentString = serializable.Content;
            Role = serializable.Role;
            Timestamp = serializable.Timestamp;
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }


        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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

    public partial class Conversation : ObservableCollection<ChatItem>
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private string SubjectString { get; set; }
        private Client OllamaConnection { get; set; }

        public string SelectedModel { get; set; }
        public DateTime Timestamp { get; set; }

        public string Subject
        {
            get => SubjectString;
            set
            {
                SubjectString = value;
                OnPropertyChanged(new("Subject"));
            }
        }

        public event PropertyChangedEventHandler? ContentChanged;
        public event EventHandler? EndOfResponse;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        public Conversation(Client conn, string model)
        {
            CancellationTokenSource = new();
            SubjectString = "New conversation";
            OllamaConnection = conn;
            SelectedModel = model;
            Timestamp = DateTime.Now;
            CollectionChanged += OnCollectionChanged;
        }

        public Conversation(Client conn, ConversationSerializable serializable)
        {
            CancellationTokenSource = new();
            SelectedModel = serializable.SelectedModel;
            Timestamp = serializable.Timestamp;
            SubjectString = serializable.Subject;
            OllamaConnection = conn;
            CollectionChanged += OnCollectionChanged;
            foreach (ChatItemSerializable item in serializable.Messages)
            {
                ChatItem chatItem = new(item);
                chatItem.PropertyChanged += OnContentChanged;
                Add(chatItem);
            }
        }

        protected void OnContentChanged(object? sender, PropertyChangedEventArgs e)
        {
            ContentChanged?.Invoke(sender, e);
        }

        protected void OnEndOfResponse(object? sender, EventArgs e)
        {
            EndOfResponse?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        protected void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (ChatItem newItem in e.NewItems)
                {

                    //Add listener for each item on PropertyChanged event
                    newItem.PropertyChanged += OnContentChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (ChatItem oldItem in e.OldItems)
                {
                    oldItem.PropertyChanged -= OnContentChanged;
                }
            }
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new();
        }

        public async Task SendUserMessage(string content)
        {
            //return early if no model selected
            if (SelectedModel == null) return;
            //set subject if new conversation
            if (Subject == "New conversation")
            {
                if (content.Length > 30) Subject = content.Substring(0, 30);
                else Subject = content;
            }

            //add user message
            Add(new(Role.user, content));

            //initialize assistant response with empty content
            ChatItem responseChatItem = new(Role.assistant, "");

            //add assistant message
            Add(responseChatItem);

            //build HTTP request data
            ChatRequest request = new()
            {
                model = SelectedModel,
                messages = this.Select(m => m.ToMessage()).ToArray(),
                stream = true
            };

            //Make HTTP POST request to ollama with built request, cancel if necessary
            try
            {
                await Task.Run(async () =>
                {
                    //Send HTTP request and read JSON stream, passing parsed objects to responseChatItem via an IProgress<ChatResponse> interface
                    if (await OllamaConnection.ChatStream(request) is DelimitedJsonStream<ChatResponse> stream)
                    {
                        using (stream)
                        {
                            await stream.Read(responseChatItem.Progress, CancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                });

                OnEndOfResponse(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
        }
    }

    public class Conversations : ObservableCollection<Conversation>
    {
        public event EventHandler? Loaded;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        public ObservableCollection<string> AvailableModels { get; set; }

        private Client OllamaConnection { get; set; }

        public Conversations(TimeSpan? HttpRequestTimeout = null)
        {
            AvailableModels = [];
            
            if(HttpRequestTimeout != null)
            {
                OllamaConnection = new Client(HttpRequestTimeout);
            }
            else
            {
                OllamaConnection = new Client(Timeout.InfiniteTimeSpan);
            }
        }

        protected void OnLoaded(object? sender, EventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }   

        public void New(string model)
        {
            Conversation newConv = new(OllamaConnection, model);

            newConv.EndOfResponse += Conversation_EndOfResponse;

            Add(newConv);
        }

        public async Task LoadAvailableModels()
        {
            try
            {
                List<string> results = [];

                await Task.Run(async () =>
                {
                    if (await OllamaConnection.ListModels() is ListModelsResponse response && response.models is ModelInfo[] models)
                    {
                        foreach (ModelInfo model in models)
                        {
                            if (model.model != null) results.Add(model.model);
                        }
                    }
                });

                AvailableModels = new(results);

                OnLoaded(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));
            }
        }

        public async Task LoadSavedConversations()
        {
            try
            {
                List<Conversation> results = [];

                await Task.Run(async () =>
                {
                    AppState state = await LocalStorage.GetSavedAppState();

                    foreach (ConversationSerializable c in state.Conversations)
                    {
                        Conversation newConv = new Conversation(OllamaConnection, c);

                        newConv.EndOfResponse += Conversation_EndOfResponse;

                        results.Add(newConv);
                    }
                });

                foreach(Conversation c in results)
                {
                    Add(c);
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(e, false));  
            }
        }

        private async void Conversation_EndOfResponse(object? sender, EventArgs e)
        {
            try
            {
                await Task.Run(async () =>
                {
                    if (!LocalStorage.IsSavingData)
                    {
                        await LocalStorage.SaveConversations(this);
                    }
                });
            }
            catch (COMException ex)
            {
                Debug.Write(ex);
                OnUnhandledException(this, new UnhandledExceptionEventArgs(ex, false));
            }
        }
    }

    [DataContract]
    public class ChatItemSerializable(ChatItem input)
    {
        [DataMember]
        public string Role { get; set; } = input.Role;
        [DataMember]
        public DateTime Timestamp { get; set; } = input.Timestamp;
        [DataMember]
        public string Content { get; set; } = input.Content;
    }

    [DataContract]
    public class ConversationSerializable(Conversation input)
    {
        [DataMember]
        public string SelectedModel { get; set; } = input.SelectedModel;
        [DataMember]
        public DateTime Timestamp { get; set; } = input.Timestamp;
        [DataMember]
        public string Subject { get; set; } = input.Subject;
        [DataMember]
        public ChatItemSerializable[] Messages { get; set; } = input.Select(m => new ChatItemSerializable(m)).ToArray();
    }

    public partial class ChatItemTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChatItem item)
            {
                return item.Timestamp.ToLocalTime().ToShortDateString() + " " + item.Timestamp.ToLocalTime().ToShortTimeString();
            }
            else return DateTime.Now.ToLocalTime().ToShortDateString() + " " + DateTime.Now.ToLocalTime().ToShortTimeString();
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
}
