using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TextBoxDialog : Page
    {
        public string InputText => InputTextBox.Text;

        public TextBoxDialog(Paragraph content, string placeholderText)
        {
            InitializeComponent();
            ContentTextBlock.Blocks.Add(content);
            InputTextBox.PlaceholderText = placeholderText;
        }
    }
}
