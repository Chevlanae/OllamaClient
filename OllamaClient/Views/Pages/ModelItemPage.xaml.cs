using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Services;
using OllamaClient.ViewModels;
using OllamaClient.Views.Dialogs;
using System;

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
            if (e.Parameter is ModelItemPageNavigationArgs args)
            {
                DispatcherQueue = args.DispatcherQueue;
                Item = args.SelectedItem;
                ParentCollection = args.Collection;

                Item.UnhandledException += Item_UnhandledException;

                ItemGrid.DataContext = Item;

                DetailsTextBox.Blocks.Add(Item.DetailsParagraph);
                ModelInfoTextBox.Blocks.Add(Item.ModelInfoParagraph);
                LicenseTextBox.Blocks.Add(Item.LicenseParagraph);
                ModelFileTextBox.Blocks.Add(Item.ModelFileParagraph);

                DispatcherQueue.TryEnqueue(() => Item.GenerateParagraphText());
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (Item is not null)
            {
                Item.UnhandledException -= Item_UnhandledException;

                DetailsTextBox.Blocks.Clear();
                ModelInfoTextBox.Blocks.Clear();
                LicenseTextBox.Blocks.Clear();
                ModelFileTextBox.Blocks.Clear();
            }
        }

        private void Item_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ErrorPopupContentDialog dialog = new(XamlRoot, (Exception)e.ExceptionObject);

            DispatcherQueue?.TryEnqueue(async () => { await DialogService.ShowDialog(dialog); });
        }

        private async void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            DeleteModelContentDialog dialog = new(XamlRoot, Item?.Model ?? "");

            ContentDialogResult? result = await DialogService.ShowDialog(dialog);

            if (result == ContentDialogResult.Primary && ParentCollection is not null && Item is not null)
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.DeleteModel(Item.Model); });
            }
        }

        private async void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            CopyModelContentDialog dialog = new(XamlRoot, Item?.Model ?? "");

            ContentDialogResult? result = await DialogService.ShowDialog(dialog);

            if
            (
                result == ContentDialogResult.Primary
                &&
                (dialog.Content as TextBoxDialog)?.InputText is string newModelName
                &&
                ParentCollection is not null
                &&
                Item is not null
            )
            {
                DispatcherQueue?.TryEnqueue(async () => { await ParentCollection.CopyModel(Item.Name, newModelName); });
            }
        }
    }
}
