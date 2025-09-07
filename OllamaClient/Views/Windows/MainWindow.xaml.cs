using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using OllamaClient.ViewModels;
using OllamaClient.Views.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainWindow : Window
    {
        private class NavActivation : INotifyPropertyChanged
        {
            private bool _Conversations { get; set; } = false;
            private bool _Models { get; set; } = false;
            private bool _Tools { get; set; } = false;
            private bool _Settings { get; set; } = false;

            public bool Conversations
            {
                get => _Conversations;
                set {
                    _Conversations = value;
                    OnPropertyChanged();
                }
            }

            public bool Models
            {
                get => _Models;
                set
                {
                    _Models = value;
                    OnPropertyChanged();
                }
            }

            public bool Tools
            {
                get => _Tools;
                set
                {
                    _Tools = value;
                    OnPropertyChanged();
                }
            }

            public bool Settings
            {
                get => _Settings;
                set
                {
                    _Settings = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName]string? caller = null) => PropertyChanged?.Invoke(this, new(caller));

            public void Reset()
            {
                Conversations = false;
                Models = false;
                Tools = false;
                Settings = false;
            }
        }

        private NavActivation _NavActivation;
        private SerilogObserverViewModel _SerilogObserver;

        public MainWindow()
        {
            InitializeComponent();

            ContentFrame.Navigate(typeof(BlankPage));

            _NavActivation = new();
            _SerilogObserver = new(DispatcherQueue);

            _NavActivation.PropertyChanged += _NavActivation_PropertyChanged;

            App.LogEvents?.Subscribe(_SerilogObserver);

            LogsItemsView.ItemsSource = _SerilogObserver.Logs;
        }

        private void _NavActivation_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if(sender is NavActivation navActivation)
            {
                switch (e.PropertyName)
                {
                    case "Conversations":
                        ConversationsButton.Background = navActivation.Conversations ? new SolidColorBrush((Color)Application.Current.Resources["SolidBackgroundFillColorBase"]) : null;
                        break;
                    case "Models":
                        ModelsButton.Background = navActivation.Models ? new SolidColorBrush((Color)Application.Current.Resources["SolidBackgroundFillColorBase"]) : null;
                        break;
                    case "Tools":
                        ToolsButton.Background = navActivation.Tools ? new SolidColorBrush((Color)Application.Current.Resources["SolidBackgroundFillColorBase"]) : null;
                        break;
                    case "Settings":
                        break;

                }
            }
        }

        private void ToggleSidebar()
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ConversationSidebarPage))
            {
                _NavActivation.Reset();
                _NavActivation.Conversations = true;

                ConversationSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ConversationSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ModelSidebarPage))
            {
                _NavActivation.Reset();
                _NavActivation.Models = true;

                ModelSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ModelSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ToolsSidebarPage))
            {
                _NavActivation.Reset();
                _NavActivation.Tools = true;

                ToolsSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ToolsSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleSidebar();
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            LogsPopup.IsOpen = !LogsPopup.IsOpen;
        }

        private async void LogsFolderHyperlink_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(App.LogsDirectoryMsixPath);

            await Launcher.LaunchFolderAsync(folder);
        }

        private void LogsPopup_Opened(object sender, object e)
        {
            LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
        }

        private void LogsItemsView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (LogsScrollViewer.VerticalOffset > LogsScrollViewer.ScrollableHeight - 300)
            {
                LogsScrollViewer.ScrollToVerticalOffset(LogsScrollViewer.ScrollableHeight);
            }
        }

        private void CopyKeyboardAccelerator_Invoked(Microsoft.UI.Xaml.Input.KeyboardAccelerator sender, Microsoft.UI.Xaml.Input.KeyboardAcceleratorInvokedEventArgs args)
        {
            StringBuilder builder = new();
            foreach (var item in LogsItemsView.SelectedItems)
            {
                switch (item)
                {
                    case char character:
                        if (character == '\u00A0')
                        {
                            builder.Append("\n");
                        }
                        else
                        {
                            builder.Append(character);
                        }
                        break;
                    case string stringObj:
                        builder.Append(stringObj);
                        break;
                }
            }
            DataPackage package = new()
            {
                RequestedOperation = DataPackageOperation.Copy
            };

            package.SetText(builder.ToString());

            Clipboard.SetContent(package);
        }
    }
}

