using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    public class ModelItemPageNavigationArgs(DispatcherQueue dispatcherQueue, ModelItem modelItem, ModelCollection collection)
    {
        public DispatcherQueue DispatcherQueue { get; set; } = dispatcherQueue;
        public ModelItem SelectedItem { get; set; } = modelItem;
        public ModelCollection Collection { get; set; } = collection;
    }

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelItemPage : Page
    {
        private new DispatcherQueue? DispatcherQueue { get; set; }
        private ModelItem? Item { get; set; }
        private ModelCollection? ParentCollection { get; set; }

        public ModelItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter is ModelItemPageNavigationArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
                Item = args.SelectedItem;
                ParentCollection = args.Collection;

                ItemGrid.DataContext = Item;
                DetailsTextBox.Text = Item.ToDetailsString();
            }
        }

        private void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if(ParentCollection is not null && Item is not null)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.DeleteModel(Item.Model); });
            }
        }

        private void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            
        }

        private void InfoButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }
    }
}
