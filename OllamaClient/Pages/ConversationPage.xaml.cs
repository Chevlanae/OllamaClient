using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        private Conversation? Session { get; set; }
        private bool IsScrolling 
        {
            get
            {
                return ChatItemsView.ScrollView.State != ScrollingInteractionState.Idle;
            }
        }

        public ConversationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Conversation conversation)
            {
                Session = conversation;
                ChatItemsView.ItemsSource = Session;
            }
            base.OnNavigatedTo(e);
        }

        private async void SendChatButton_Click(object? sender, RoutedEventArgs e)
        {
            if(Session != null)
            {
                ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);

                ChatInputTextBox.IsEnabled = false;
                SendChatButton.IsEnabled = false;

                string text = ChatInputTextBox.Text;

                await Session.NewUserMessage(text);

                ChatInputTextBox.Text = "";
                ChatInputTextBox.IsEnabled = true;
                SendChatButton.IsEnabled = true;
            }
        }

        private void CancelChatButton_Click(object sender, RoutedEventArgs e)
        {
            if(Session != null) Session.Cancel();
        }

        private void ChatBubbleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(!IsScrolling)
            {
                ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);
            }
        }

        private void ChatItemsView_Loaded(object sender, RoutedEventArgs e)
        {
            ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);
        }
    }
}
