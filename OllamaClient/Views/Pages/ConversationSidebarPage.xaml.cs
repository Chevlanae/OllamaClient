using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Models;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;

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

        private ConversationSidebarViewModel? _ConversationsSidebarViewModel { get; set; }

        public ConversationSidebarPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                if(_ConversationsSidebarViewModel is null)
                {
                    _ConversationsSidebarViewModel = new(args.ContentFrame, XamlRoot, DispatcherQueue, ConversationsListView);
                }
                ConversationsListView.ItemsSource = _ConversationsSidebarViewModel.ConversationViewModelCollection;
                ConversationsListView.SelectedIndex = -1;
            }

            base.OnNavigatedTo(e);
        }

        private void ConversationsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ConversationsSidebarViewModel?.ConversationsListView_SelectionChanged();
        }

        private void DeleteConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is AppBarButton button && button.DataContext is ConversationViewModel c)
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
    }
}
