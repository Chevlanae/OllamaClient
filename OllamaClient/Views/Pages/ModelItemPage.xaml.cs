using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
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
    public sealed partial class ModelItemPage : Page
    {
        public class NavArgs(ModelViewModel modelItem, ModelSidebarViewModel viewModel)
        {
            public ModelViewModel SelectedItem { get; set; } = modelItem;
        }

        private ModelViewModel? ModelViewModel { get; set; }

        public ModelItemPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                ModelViewModel = args.SelectedItem;

                ItemGrid.DataContext = ModelViewModel;

                DetailsTextBox.Blocks.Add(ModelViewModel.DetailsParagraph);
                ModelInfoTextBox.Blocks.Add(ModelViewModel.ModelInfoParagraph);
                LicenseTextBox.Blocks.Add(ModelViewModel.LicenseParagraph);
                ModelFileTextBox.Blocks.Add(ModelViewModel.ModelFileParagraph);
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            DetailsTextBox.Blocks.Clear();
            ModelInfoTextBox.Blocks.Clear();
            LicenseTextBox.Blocks.Clear();
            ModelFileTextBox.Blocks.Clear();

            base.OnNavigatedFrom(e);
        }

        private void DeleteButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ModelViewModel?.ShowDeleteDialog();
        }

        private void CopyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ModelViewModel?.ShowCopyDialog();
        }
    }
}
