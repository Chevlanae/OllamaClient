using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ConversationPageNavigationArgs(Conversation conversation, DispatcherQueue dispatcherQueue)
    {
        public Conversation Conversation { get; set; } = conversation;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class ConversationPage : Page
    {
        private Conversation? Conversation { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }

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
            if (e.Parameter is ConversationPageNavigationArgs args)
            {
                Conversation = args.Conversation;
                DispatcherQueue = args.DispatcherQueue;
                Conversation.UnhandledException += Conversation_UnhandledException;
                ChatItemsView.ItemsSource = Conversation;
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
            if (Conversation != null && DispatcherQueue != null)
            {
                ChatItemsView.ScrollView.ScrollTo(0, ChatItemsView.ScrollView.ScrollableHeight);

                ChatInputTextBox.IsEnabled = false;
                SendChatButton.IsEnabled = false;

                string text = ChatInputTextBox.Text;

                DispatcherQueue.TryEnqueue(async () => { await Conversation.SendUserMessage(text); });

                ChatInputTextBox.Text = "";
                ChatInputTextBox.IsEnabled = true;
                SendChatButton.IsEnabled = true;

                ChatItemsView_ScrollToBottom(sender, e);
            }
        }

        private void CancelChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (Conversation != null) Conversation.Cancel();
        }

        private void ChatItemsView_Loaded(object sender, RoutedEventArgs e)
        {
            ChatItemsView_ScrollToBottom(sender, e);
        }

        private void ChatItemsView_ScrollToBottom(object? sender, RoutedEventArgs e)
        {
            if (!IsScrolling)
            {
                ChatItemsView.ScrollView.ScrollTo(ChatItemsView.ScrollView.HorizontalOffset, ChatItemsView.ScrollView.ScrollableHeight);
            }
        }
    }
}
