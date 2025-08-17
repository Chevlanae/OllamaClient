using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using OllamaClient.ViewModels;

namespace OllamaClient.Views.Dialogs
{
    public class CreateModelContentDialog : ContentDialog
    {
        public CreateModelContentDialog(XamlRoot xamlRoot, ModelSidebarViewModel viewModel)
        {
            Title = $"Create Model";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Create";
            CloseButtonText = "Cancel";
            Content = new CreateModelDialog(viewModel);
            XamlRoot = xamlRoot;
        }
    }
}
