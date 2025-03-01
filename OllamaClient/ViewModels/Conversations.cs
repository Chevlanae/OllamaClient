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
        private bool RingEnabled { get; set; } = false;

        [DataMember]
        private DateTime? MessageTimestamp { get; set; }
        [DataMember]
        private string ContentString { get; set; } = "";
        [DataMember]
        public string Role { get; set; } = "user";

        public IProgress<ChatResponse> Progress;

        public bool ProgressRingEnabled
        {
            get
            {
                return RingEnabled;
            }
            set
            {
                RingEnabled = value;
                OnPropertyChanged(this, new("ProgressRingEnabled"));
            }
        }

        public DateTime? Timestamp
        {
            get
            {
                return MessageTimestamp;
            }
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

        public ChatItem()
        {
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }

        public ChatItem(Role role, string content) : this(new Message(role, content)) { }

        public ChatItem(Message message)
        {
            ContentString = message.content ?? "";
            Role = message.role ?? "user";
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }

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
        private string SubjectString { get; set; } = "New conversation";
        [DataMember]
        public string SelectedModel { get; set; } = "";
        [DataMember]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        [DataMember]
        public ObservableCollection<ChatItem> Items { get; set; } = [];

        public string Subject
        {
            get => SubjectString;
            set
            {
                SubjectString = value;
                OnPropertyChanged(this, new("Subject"));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? StartOfRequest;
        public event EventHandler? EndOfResponse;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;

        public Conversation() { }

        public Conversation(string model)
        {
            SelectedModel = model;
        }

        protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        protected void OnStartOfRequest(object? sender, EventArgs e)
        {
            StartOfRequest?.Invoke(sender, e);
        }

        protected void OnEndOfResponse(object? sender, EventArgs e)
        {
            EndOfResponse?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        public void Cancel()
        {
            if(CancellationTokenSource != null)
            {
                CancellationTokenSource.Cancel();
            }
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

            if(CancellationTokenSource == null) CancellationTokenSource = new();

            ChatItem newChatItem = new(Role.user, content);

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

            OnStartOfRequest(this, EventArgs.Empty);

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
                OnEndOfResponse(this, EventArgs.Empty);
            }
        }
    }

    [KnownType(typeof(ChatItem))]
    [KnownType(typeof(Conversation))]
    [DataContract]
    public class Conversations : INotifyPropertyChanged
    {
        public event EventHandler? Loaded;
        public event EventHandler<UnhandledExceptionEventArgs>? UnhandledException;
        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<string> AvailableModels { get; set; } = [];
        [DataMember]
        public ObservableCollection<Conversation> Items { get; set; } = [];

        protected void OnLoaded(object? sender, EventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        protected void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            UnhandledException?.Invoke(sender, e);
        }

        protected void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private async void Conversation_StartOfRequest(object? sender, EventArgs e)
        {
            await Save();
        }

        private async void Conversation_EndOfResponse(object? sender, EventArgs e)
        {
            await Save();
        }

        public void Create(string? model)
        {
            Conversation newConv;

            if (model == null) newConv = new();
            else newConv = new(model);

            newConv.StartOfRequest += Conversation_StartOfRequest;
            newConv.EndOfResponse += Conversation_EndOfResponse;

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
                Conversation[] result = [];

                await Task.Run(async () =>
                {
                    if (await LocalStorage.Get(typeof(Conversations)) is Conversations savedConvos && savedConvos.Items != null)
                    {
                        result = savedConvos.Items.ToArray();
                    }
                });

                foreach (Conversation c in result)
                {
                    Items.Add(c);
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
                    await LocalStorage.Save(this);
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
}
