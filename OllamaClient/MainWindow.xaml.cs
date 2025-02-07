using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using OllamaClient.Api;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<string> AvailableModels { get; set; }
        private Connection? OllamaConnection { get; set; }
        private ObservableCollection<Conversation> Conversations { get; set; }
        private ObservableCollection<string> SocketAddresses { get; set; }

        public MainWindow()
        {
            AvailableModels = [];
            Conversations = [];
            SocketAddresses = [];

            InitializeComponent();

            ModelsComboBox.ItemsSource = AvailableModels;
            ConversationsListBox.ItemsSource = Conversations;

            Conversations.CollectionChanged += Conversations_CollectionChanged;
        }

        private async void Conversations_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await Config.SaveAppState(new(SocketAddresses.ToArray(), Conversations.ToArray()));
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
            ConversationsListBox.Visibility = Visibility.Visible;
            ModelsComboBox.Visibility = Visibility.Visible;
            SocketAddressInputTextBox.Visibility = Visibility.Visible;
        }

        private void TopLevelSplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args)
        {
            ConversationsListBox.Visibility = Visibility.Collapsed;
            ModelsComboBox.Visibility = Visibility.Collapsed;
            SocketAddressInputTextBox.Visibility = Visibility.Collapsed;
        }

        private void NewConversationButton_Click(object sender, RoutedEventArgs e)
        {
            if (ModelsComboBox.SelectedItem is string model && OllamaConnection != null)
            {
                Conversation newConversation = new (OllamaConnection, model);

                Conversations.Add(newConversation);

                ConversationsListBox.SelectedIndex = Conversations.Count - 1;
            }
        }

        private void SocketAddressInputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            SocketAddressInputTextBox.Text = OllamaConnection?.SocketAddress;
        }

        private async void SocketAddressInputTextBox_KeyUp(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                SocketProgressRing.IsActive = true;

                OllamaConnection = new(SocketAddressInputTextBox.Text);

                foreach (Conversation conversation in Conversations)
                {
                    conversation.OllamaConnection = OllamaConnection;
                }

                AvailableModels.Clear();

                try
                {
                    if (await OllamaConnection.ListModels() is ListModelsResponse response && response.models is ModelInfo[] models)
                    {
                        foreach (ModelInfo model in models)
                        {
                            if (model.model is not null)
                            {
                                AvailableModels.Add(model.model);
                            }
                        }

                        if(!SocketAddresses.Contains(SocketAddressInputTextBox.Text)) SocketAddresses.Add(SocketAddressInputTextBox.Text);
                        SocketAddressInputTextBox.BorderBrush = new SolidColorBrush(Colors.Transparent);
                        ModelsComboBox.SelectedIndex = 0;
                    }
                }
                catch(HttpRequestException ex)
                {
                    SocketAddressInputTextBox.BorderBrush = new SolidColorBrush(Colors.Red);
                    Debug.Write(ex);
                }

                SocketProgressRing.IsActive = false;
            }
        }

        private void ConversationsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is ListBox listBox && listBox.SelectedItem is Conversation conversation)
            {
                ContentFrame.Navigate(typeof(ChatSessionPage), conversation);
            }
        }

        private void SocketAddressInputTextBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Since selecting an item will also change the text,
            // only listen to changes caused by user entering text.
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var suitableItems = new List<string>();
                foreach (string address in SocketAddresses)
                {
                    if (address.Contains(SocketAddressInputTextBox.Text))
                    {
                        suitableItems.Add(address);
                    }
                }
                sender.ItemsSource = suitableItems;
            }

        }

        private void SocketAddressInputTextBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            SocketAddressInputTextBox.Text = args.SelectedItem.ToString();

        }
    }
}

