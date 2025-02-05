using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models;
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
    public partial class ChatSessionPage : Page
    {

        private ChatSession Session { get; set; }

        public ChatSessionPage()
        {
            Session = new(new("127.0.0.1:4443", new(1, 0, 0)));
            Session.ContentRecieved += Session_ContentRecieved;

            InitializeComponent();


            ChatItemsView.ItemsSource = Session;
        }

        private void Session_ContentRecieved(object? sender, EventArgs e)
        {
            // Scroll to the last item
            ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is string model)
            {
                Session.SelectedModel = model;
            }
            base.OnNavigatedTo(e);
        }

        private void SendChatButton_Click(object sender, RoutedEventArgs e)
        {
            ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);

            ChatInputTextBox.IsEnabled = false;
            SendChatButton.IsEnabled = false;

            string text = ChatInputTextBox.Text;


            DispatcherQueue.TryEnqueue(async () => { await Session.NewUserMessage(text); });


            ChatInputTextBox.Text = "";

            ChatInputTextBox.IsEnabled = true;
            SendChatButton.IsEnabled = true;
        }

        private void NewConversationButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Cancel();
            Session.Clear();
        }

        private void CancelChatButton_Click(object sender, RoutedEventArgs e)
        {
            Session.Cancel();
        }
    }
}
