using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TemplateContentPage : Page
    {
        private CreateModelDialog.InputResults? Results { get; set; }

        public TemplateContentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CreateModelDialog.DialogArgs args)
            {
                Results = args.Results;

                InputTextBox.Text = Results?.From?.Template;
            }

            base.OnNavigatedTo(e);
        }

        private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox inputTextBox && Results is not null)
            {
                Results.Template = inputTextBox.Text;
            }
        }
    }
}
