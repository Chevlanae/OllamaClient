using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Views.Dialogs
{
    public class CreateModelContentDialog : ContentDialog
    {
        public class DialogArgs
        {
            public string[] AvailableModels { get; set; }
        }

        public CreateModelContentDialog(XamlRoot xamlRoot, DialogArgs args)
        {
            Title = $"Create Model";
            DefaultButton = ContentDialogButton.Primary;
            PrimaryButtonText = "Create";
            CloseButtonText = "Cancel";
            Content = new CreateModelDialog(args);
            XamlRoot = xamlRoot;
        }
    }
}
