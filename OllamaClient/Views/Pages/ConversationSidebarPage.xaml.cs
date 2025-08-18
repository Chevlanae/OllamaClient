using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ConversationSidebarPage : Page
    {
        public class NavArgs(Frame contentFrame)
        {
            public Frame ContentFrame { get; set; } = contentFrame;
        }

        private Frame? _ContentFrame { get; set; }
        private ConversationSidebarViewModel? _ConversationsSidebarViewModel { get; set; }

        public ConversationSidebarPage()
        {
            InitializeComponent();

            Loaded += ConversationSidebarPage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                _ContentFrame = args.ContentFrame;
            }

            SetViewModel();

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ConversationsListView.SelectedItem = null;
            base.OnNavigatedFrom(e);
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ConversationsSidebarViewModel?.ConversationsListView_SelectionChanged();
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.DataContext is ConversationViewModel c)
            {
                _ConversationsSidebarViewModel?.DeleteConversation(c);
            }
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            _ConversationsSidebarViewModel?.NewConversation();
        }

        private void RefreshConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            _ConversationsSidebarViewModel?.RefreshConversations();
        }

        private void ConversationSidebarPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetViewModel();
        }

        private void SetViewModel()
        {
            if (_ConversationsSidebarViewModel is null && _ContentFrame is not null && XamlRoot is not null)
            {
                _ConversationsSidebarViewModel = new(_ContentFrame, XamlRoot, DispatcherQueue, ConversationsListView);
                ConversationsListView.ItemsSource = _ConversationsSidebarViewModel.ConversationViewModelCollection;
                SidebarProgressRing.Visibility = Visibility.Visible;
                SidebarProgressRing.IsActive = true;
                ConversationsListView.Visibility = Visibility.Collapsed;
                _ConversationsSidebarViewModel.ConversationsLoaded += _ConversationsSidebarViewModel_ConversationsLoaded;
            }
        }

        private void _ConversationsSidebarViewModel_ConversationsLoaded(object? sender, System.EventArgs e)
        {
            SidebarProgressRing.Visibility = Visibility.Collapsed;
            SidebarProgressRing.IsActive = false;
            ConversationsListView.Visibility = Visibility.Visible;
        }
    }
}
