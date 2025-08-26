using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace OllamaClient.Views.Dialogs
{
    class CreateNewToolContentDialog : ContentDialog
    {
        public CreateNewToolContentDialog(XamlRoot xamlRoot)
        {
            Title = $"Create New Tool";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Create";
            CloseButtonText = "Cancel";
            Content = new CreateNewToolDialog();
            XamlRoot = xamlRoot;
        }
    }
}
