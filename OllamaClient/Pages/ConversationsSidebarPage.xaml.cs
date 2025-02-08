using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using OllamaClient.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationsSidebarPage : Page
    {
        private Frame? ContentFrame { get; set; }
        private Conversations Conversations { get; set; }

        public ConversationsSidebarPage()
        {
            Conversations = [];
            Conversations.Loaded += Conversations_Loaded;

            InitializeComponent();

            ConversationsListView.ItemsSource = Conversations;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is Frame contentFrame)
            {
                ContentFrame = contentFrame;
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadSavedConversations(); });
                DispatcherQueue.TryEnqueue(async () => { await Conversations.LoadAvailableModels(); });
                ModelsComboBox.ItemsSource = Conversations.AvailableModels;
            }
            base.OnNavigatedTo(e);
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
            if (ConversationsListView.SelectedItem is Conversation conversation)
            {
                ContentFrame?.Navigate(typeof(ConversationPage), conversation);
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
