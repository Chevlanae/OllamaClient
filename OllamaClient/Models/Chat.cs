using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using OllamaClient.Api;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;

namespace OllamaClient.Models
{
    public partial class ChatItem : INotifyPropertyChanged
    {
        private string _Content { get; set; }
        private bool _IsSendingMessage { get; set; }

        public string Role { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Content
        {
            get => _Content;
            set
            {
                _Content = value;
                OnPropertyChanged();
            }
        }

        public IProgress<ChatResponse> Progress { get; set; }

        public ChatItem(Role role, string content) : this(new Message(role, content)) { }

        public ChatItem(Message message)
        {
            _Content = message.content ?? "";
            Role = message.role ?? "user";
            Timestamp = DateTime.Now;
            Progress = new Progress<ChatResponse>(s => Content = Content + s.message?.content ?? "");
        }
        public event PropertyChangedEventHandler? PropertyChanged;

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

    public partial class ChatSession(Connection connection) : ObservableCollection<ChatItem>
    {
        private CancellationTokenSource CancellationTokenSource { get; set; } = new();
        private Connection OllamaConnection { get; set; } = connection;

        public event EventHandler? ContentRecieved;

        public string? Subject { get; set; }
        public string? SelectedModel { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;

        protected void OnContentRecieved(object? sender, EventArgs e)
        {
            ContentRecieved?.Invoke(sender, e);
        }

        public void Cancel()
        {
            CancellationTokenSource.Cancel();
            CancellationTokenSource = new();
        }

        public async Task NewUserMessage(string content)
        {
            if(SelectedModel == null) return;

            Add(new(Role.user, content));

            ChatRequest request = new()
            {
                model = SelectedModel,
                messages = this.Select(m => m.ToMessage()).ToArray(),
                stream = true
            };

            ChatItem responseChatItem = new(Role.assistant, "");

            responseChatItem.PropertyChanged += OnContentRecieved;

            Add(responseChatItem);

            await Task.Run(async () =>
            {
                if (await OllamaConnection.ChatStream(request) is DelimitedJsonStream<ChatResponse> stream)
                {
                    using (stream)
                    {
                        try
                        {
                            await stream.Read(CancellationTokenSource.Token, responseChatItem.Progress).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException e)
                        {
                            stream.Dispose();
                            Debug.Write(e);
                        }
                    }
                }
            }, CancellationTokenSource.Token);
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
