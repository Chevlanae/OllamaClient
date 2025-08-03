
using OllamaClient.Json;
using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public partial class ChatMessageViewModel : INotifyPropertyChanged
    {
        private ChatMessage _ChatMessage;
        private bool _ProgressRingEnabled { get; set; }

        public ChatMessageViewModel(Role role, string content, DateTime? timeStamp = null, bool progressRingEnabled = false)
        {
            _ChatMessage = new(role, content, timeStamp);
            _ProgressRingEnabled = progressRingEnabled;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

        public string Role
        {
            get => Enum.GetName(_ChatMessage.Role) ?? "user";
            set
            {
                if (Enum.TryParse(value, out Role role))
                {
                    _ChatMessage.Role = role;
                }
            }
        }

        public bool ProgressRingEnabled
        {
            get => _ProgressRingEnabled;
            set
            {
                _ProgressRingEnabled = value;
                OnPropertyChanged();
            }
        }

        public string? Timestamp
        {
            get
            {
                if (_ChatMessage.Timestamp is not null)
                    return _ChatMessage.Timestamp?.ToLocalTime().ToShortDateString() + " " + _ChatMessage.Timestamp?.ToLocalTime().ToShortTimeString();
                else return default;
            }
        }

        public string Content
        {
            get => _ChatMessage.Content;
            set
            {
                _ChatMessage.Content = value;
                OnPropertyChanged();
            }
        }

        public string HorizontalAlignment
        {
            get => _ChatMessage.Role == Json.Role.assistant ? "Left" : "Right";
        }

        public ChatMessage ToChatMessage() => _ChatMessage;

        public Message ToMessage() => _ChatMessage.ToMessage();

        public void SetTimestamp(DateTime dateTime)
        {
            if (_ChatMessage is not null)
            {
                _ChatMessage.Timestamp = dateTime;
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }
}
