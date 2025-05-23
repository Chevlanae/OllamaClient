﻿using OllamaClient.Models;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace OllamaClient.ViewModels
{
    [DataContract]
    public partial class ChatMessage : INotifyPropertyChanged
    {
        private bool _ProgressRingEnabled { get; set; } = false;

        private Role _Role { get; set; } = OllamaClient.Models.Role.user;

        [DataMember]
        private DateTime? _Timestamp { get; set; }
        [DataMember]
        private string _String { get; set; } = "";

        [DataMember]
        public string Role
        {
            get => Enum.GetName(_Role) ?? "user";
            set
            {
                if (Enum.TryParse(value, out Role role))
                {
                    _Role = role;
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
            get => _Timestamp?.ToLocalTime().ToShortDateString() + " " + _Timestamp?.ToLocalTime().ToShortTimeString();
        }

        public string Content
        {
            get => _String;
            set
            {
                _String = value;
                OnPropertyChanged();
            }
        }

        public string HorizontalAlignment
        {
            get => _Role == OllamaClient.Models.Role.assistant ? "Left" : "Right";
        }

        public string BackgroundColor
        {
            get => _Role == OllamaClient.Models.Role.assistant ? "Transparent" : "DimGray";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new(name));
        }

        public void SetTimestamp(DateTime dateTime)
        {
            _Timestamp = dateTime;
            OnPropertyChanged(nameof(Timestamp));
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
}
