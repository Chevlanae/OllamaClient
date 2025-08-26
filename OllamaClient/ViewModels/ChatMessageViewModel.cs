
using OllamaClient.Services.Json;
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

        public ChatMessageViewModel(ChatMessage chatMessage, bool progressRingEnabled = false)
        {
            _ChatMessage = chatMessage;
            _ProgressRingEnabled = progressRingEnabled;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new(name));

        public Role Role
        {
            get => _ChatMessage.Role;
            set
            {
                _ChatMessage.Role = value;
                OnPropertyChanged();
            }
        }

        public DateTime? Timestamp
        {
            get => _ChatMessage.Timestamp;
            set
            {
                _ChatMessage.Timestamp = value;
                OnPropertyChanged();
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

        public bool ProgressRingEnabled
        {
            get => _ProgressRingEnabled;
            set
            {
                _ProgressRingEnabled = value;
                OnPropertyChanged();
            }
        }

        public string RoleString
        {
            get => Enum.GetName(_ChatMessage.Role) ?? "user";
        }

        public string? TimestampString
        {
            get
            {
                if (_ChatMessage.Timestamp is not null)
                    return _ChatMessage.Timestamp?.ToLocalTime().ToShortDateString() + " " + _ChatMessage.Timestamp?.ToLocalTime().ToShortTimeString();
                else return default;
            }
        }

        public string HorizontalAlignment
        {
            get => _ChatMessage.Role == Role.assistant ? "Left" : "Right";
        }

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
