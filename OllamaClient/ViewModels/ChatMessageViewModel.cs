using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OllamaClient.ViewModels
{
    public partial class ChatMessageViewModel(ChatMessage message, bool progressRingEnabled) : INotifyPropertyChanged
    {
        private ChatMessage? _ChatMessage { get; set; } = message;
        private bool _ProgressRingEnabled { get; set; } = progressRingEnabled;

        public string? Role
        {
            get
            {
                if (_ChatMessage is not null)
                {
                    return Enum.GetName(_ChatMessage.Role) ?? "user";
                }
                else return default;
            }
            set
            {
                if (_ChatMessage is not null && Enum.TryParse(value, out Role role))
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
                if (_ChatMessage is not null && _ChatMessage.Timestamp is not null)
                {
                    return _ChatMessage.Timestamp?.ToLocalTime().ToShortDateString() + " " + _ChatMessage.Timestamp?.ToLocalTime().ToShortTimeString();
                }
                else return default;
            }
        }

        public string? Content
        {
            get => _ChatMessage?.Content;
            set
            {
                if(_ChatMessage is not null)
                {
                    _ChatMessage.Content = value ?? "";
                    OnPropertyChanged();
                }
            }
        }

        public string HorizontalAlignment
        {
            get => _ChatMessage?.Role == Models.Role.assistant ? "Left" : "Right";
        }

        public string BackgroundColor
        {
            get => _ChatMessage?.Role == Models.Role.assistant ? "Transparent" : "DimGray";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public void SetChatMessage(ChatMessage chatMessage)
        {
            _ChatMessage = chatMessage;
        }

        public ChatMessage? GetChatMessage()
        {
            return _ChatMessage;
        }

        public void SetTimestamp(DateTime dateTime)
        {
            if(_ChatMessage is not null)
            {
                _ChatMessage.Timestamp = dateTime;
                OnPropertyChanged(nameof(Timestamp));
            }
        }
    }
}
