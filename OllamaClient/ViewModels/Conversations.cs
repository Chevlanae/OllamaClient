using Microsoft.UI.Xaml.Data;
using OllamaClient.Models.Ollama;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace OllamaClient.ViewModels
{
    [DataContract]
    public partial class ChatItem : INotifyPropertyChanged
    {
        [DataMember]
        private string ContentString { get; set; } = "";
        [DataMember]
        public string Role { get; set; } = "user";
        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public IProgress<ChatResponse> Progress;

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

        public ChatItem()
        {
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }

        public ChatItem(Role role, string content) : this(new Message(role, content)) { }

        public ChatItem(Message message)
        {
            ContentString = message.content ?? "";
            Role = message.role ?? "user";
            Timestamp = DateTime.Now;
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

    [KnownType(typeof(ChatItem))]
    [CollectionDataContract]
    public partial class Conversation : ObservableCollection<ChatItem>
    {
        private CancellationTokenSource CancellationTokenSource { get; set; } = new();

        [DataMember]
        private string SubjectString { get; set; } = "";
        [DataMember]
        public string SelectedModel { get; set; } = "";
        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;

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

        public Conversation()
        {
            CollectionChanged += OnCollectionChanged;
        }

        public Conversation(string model)
        {
            CancellationTokenSource = new();
            SubjectString = "New conversation";
            SelectedModel = model;
            CollectionChanged += OnCollectionChanged;
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

        public async Task SendUserMessage(Client conn, string content)
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
                    if (await conn.ChatStream(request) is DelimitedJsonStream<ChatResponse> stream)
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

    [KnownType(typeof(ChatItem))]
    [KnownType(typeof(Conversation))]
    [CollectionDataContract]
    public class Conversations : ObservableCollection<Conversation>
    {
        public event EventHandler? Loaded;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        [DataMember]
        public ObservableCollection<string> AvailableModels { get; set; } = [];

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
            Conversation newConv = new(model);

            newConv.EndOfResponse += Conversation_EndOfResponse;

            Add(newConv);
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

                foreach(string s in results)
                {
                    AvailableModels.Add(s);
                }

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
                    if (await LocalStorage.Get(typeof(Conversations)) is Conversations savedConvos)
                    {
                        foreach (Conversation c in savedConvos)
                        {
                            results.Add(c);
                        }
                    }
                });

                foreach (Conversation c in results)
                {
                    Add(c);
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

        private async void Conversation_EndOfResponse(object? sender, EventArgs e)
        {
            try
            {
                await Task.Run(async () =>
                {
                    await LocalStorage.Save(this);
                });
            }
            catch (COMException ex)
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
