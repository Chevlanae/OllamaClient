using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Views.Dialogs
{
    internal class ConfirmCreateModelContentDialog : ContentDialog
    {
        public ConfirmCreateModelContentDialog(XamlRoot xamlRoot, string modelName)
        {
            Title = $"Create '{modelName}'";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Create";
            CloseButtonText = "Cancel";
            Content = $"Are you sure you want to create this model?";
            XamlRoot = xamlRoot;
        }
    }
}
