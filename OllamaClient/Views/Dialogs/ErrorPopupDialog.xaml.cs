using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPopupDialog : Page
    {
        public ErrorPopupDialog(Paragraph body)
        {
            InitializeComponent();
            MessageBodyTextBlock.Blocks.Add(body);

        }
    }
}
