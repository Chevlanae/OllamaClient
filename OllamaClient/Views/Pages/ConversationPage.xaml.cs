using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models.Ollama;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ConversationPageNavigationArgs(Conversation conversation, DispatcherQueue dispatcherQueue, Client ollamaClient)
    {
        public Conversation Conversation { get; set; } = conversation;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public Client OllamaClient { get; set; } = ollamaClient;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        private Conversation? Conversation { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private Client? OllamaClient { get; set; }

        private bool IsScrolling
        {
            get
            {
                return ChatItemsScrollView.State != ScrollingInteractionState.Idle;
            }
        }

        public ConversationPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationPageNavigationArgs args)
            {
                Conversation = args.Conversation;
                DispatcherQueue = args.DispatcherQueue;
                OllamaClient = args.OllamaClient;
                Conversation.UnhandledException += Conversation_UnhandledException;
                ChatItemsControl.ItemsSource = Conversation.Items;
            }
            base.OnNavigatedTo(e);
        }

        private void Conversation_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            DispatcherQueue?.TryEnqueue(() =>
            {
                new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate();
            });
        }

        private void SendChatButton_Click(object? sender, RoutedEventArgs e)
        {
            if (Conversation != null && DispatcherQueue != null && OllamaClient != null)
            {
                ChatItemsControl_ScrollToBottom(sender, e);

                ChatInputTextBox.IsEnabled = false;
                SendChatButton.IsEnabled = false;

                string text = ChatInputTextBox.Text;

                DispatcherQueue.TryEnqueue(async () => { await Conversation.SendUserMessage(OllamaClient, text); });

                ChatInputTextBox.Text = "";
                ChatInputTextBox.IsEnabled = true;
                SendChatButton.IsEnabled = true;

                ChatItemsControl_ScrollToBottom(sender, e);
            }
        }

        private void CancelChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (Conversation != null) Conversation.Cancel();
        }

        private void ChatItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            ChatItemsControl_ScrollToBottom(sender, e);
        }

        private async void ChatItemsControl_ScrollToBottom(object? sender, RoutedEventArgs e)
        {
            while (IsScrolling) await Task.Delay(100);

            ChatItemsScrollView.ScrollTo(ChatItemsScrollView.HorizontalOffset, ChatItemsScrollView.ScrollableHeight);
        }

        private void ChatBubbleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ChatItemsControl_ScrollToBottom(sender, e);
        }
    }
}
