using Microsoft.UI.Xaml;
using WinUIEx;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPopupWindow : Window
    {
        public ErrorPopupWindow(string header, string body)
        {
            InitializeComponent();
            MessageHeaderTextBlock.Text = header;
            MessageBodyTextBlock.Text = body;

            this.SetWindowSize(480, 480);
        }
    }
}
