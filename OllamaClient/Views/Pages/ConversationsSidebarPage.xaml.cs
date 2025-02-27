using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Windows;
using System;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ConversationsSideBarPageNavigationArgs(Frame contentFrame, DispatcherQueue dispatcherQueue)
    {
        public Frame ContentFrame { get; set; } = contentFrame;
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private Conversations Conversations { get; set; }

        public ConversationsSidebarPage()
        {
            Conversations = new();
            Conversations.Loaded += Conversations_Loaded;
            Conversations.UnhandledException += Conversations_UnhandledException;

            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ConversationsSideBarPageNavigationArgs args)
            {
                ContentFrame = args.ContentFrame;
                DispatcherQueue = args.DispatcherQueue;
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadSavedConversations(); });
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadAvailableModels(); });

                ConversationsListView.ItemsSource = Conversations;
                ModelsComboBox.ItemsSource = Conversations.AvailableModels;
            }
            base.OnNavigatedTo(e);
        }

        private async void Conversations_UnhandledException(object? sender, System.UnhandledExceptionEventArgs e)
        {
            await Task.Run(() =>
            {
                DispatcherQueue?.TryEnqueue(() =>
                {
                    new ErrorPopupWindow("An error occurred", e.ExceptionObject.ToString() ?? "").Activate();
                });
            });
        }

        private void Conversations_Loaded(object? sender, EventArgs e)
        {
            if (Conversations.Count > 0)
            {
                ConversationsListView.SelectedIndex = 0;
            }
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConversationsListView.SelectedItem is Conversation conversation && DispatcherQueue != null)
            {
                ConversationPageNavigationArgs args = new(conversation, DispatcherQueue);

                ContentFrame?.Navigate(typeof(ConversationPage), args);
            }
        }

        private void DeleteConversation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is Conversation c)
            {
                Conversations.Remove(c);
            }
        }

        private void ModelsComboBox_DropDownClosed(object sender, object e)
        {
            if (ModelsComboBox.SelectedItem is string selectedModel)
            {
                NewConversationFlyout.Hide();

                Conversations.New(selectedModel);

                ConversationsListView.SelectedIndex = ConversationsListView.Items.Count - 1;
            }
        }
    }
}
