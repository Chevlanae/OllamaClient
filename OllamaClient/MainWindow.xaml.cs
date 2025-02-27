using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using OllamaClient.Views.Pages;

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
        private new DispatcherQueue DispatcherQueue { get; } = DispatcherQueue.GetForCurrentThread();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ToggleSidbarButton_Click(object sender, RoutedEventArgs e)
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ConversationsSidebarPage))
            {
                ConversationsSideBarPageNavigationArgs args = new(ContentFrame, DispatcherQueue);

                SidebarFrame.Navigate(typeof(ConversationsSidebarPage), args);
            }
            ToggleSidbarButton_Click(sender, e);
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

