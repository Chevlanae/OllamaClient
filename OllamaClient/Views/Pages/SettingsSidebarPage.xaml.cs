using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{

    public class SettingsSidebarPageNavigationArgs(DispatcherQueue dispatcherQueue)
    {
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsSidebarPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }

        public SettingsSidebarPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is SettingsSidebarPageNavigationArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
            }

            base.OnNavigatedTo(e);
        }

        private void SettingsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
