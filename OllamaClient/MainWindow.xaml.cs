using Microsoft.Extensions.Hosting;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using OllamaClient.Views.Pages;
using Serilog;

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
        public MainWindow()
        {
            InitializeComponent();

            ConversationsButton_Click(this, new());
        }

        private void ToggleSidebar()
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ConversationsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ConversationSidebarPage))
            {
                ConversationSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ConversationSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ModelsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SidebarFrame.CurrentSourcePageType != typeof(ModelSidebarPage))
            {
                ModelSidebarPage.NavArgs args = new(ContentFrame);

                SidebarFrame.Navigate(typeof(ModelSidebarPage), args);
            }

            if (!TopLevelSplitView.IsPaneOpen) ToggleSidebar();
        }

        private void ToggleSidbarButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

