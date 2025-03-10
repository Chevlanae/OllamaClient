using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ModelItemPageNavigationArgs(DispatcherQueue dispatcherQueue, ModelItem modelItem)
    {
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public ModelItem ModelItem { get; set; } = modelItem;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelItemPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelItem? Item { get; set; }

        public ModelItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is ModelItemPageNavigationArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
                Item = args.ModelItem;

                RootGrid.DataContext = Item;
            }
        }
    }
}
