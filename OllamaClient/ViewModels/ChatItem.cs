using OllamaClient.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
}
