using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FromContentPage : Page
    {
        private ModelSidebarViewModel? SidebarViewModel { get; set; }
        private CreateModelDialog.InputResults? Results { get; set; }

        public FromContentPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CreateModelDialog.DialogArgs args)
            {
                SidebarViewModel = args.ViewModel;
                Results = args.Results;

                SelectedModelComboBox.ItemsSource = SidebarViewModel?.ModelViewModelCollection;
                SelectedModelComboBox.SelectedIndex = 1;
            }

            base.OnNavigatedTo(e);
        }

        private void SelectedModelComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                sender is ComboBox selectedItemComboBox &&
                selectedItemComboBox.SelectedItem is ModelViewModel viewModel &&
                Results is not null
                )
            {
                Results.From = viewModel;
            }
        }



        private void ModelNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox modelNameTextBox && Results is not null)
            {
                Results.Name = modelNameTextBox.Text;
            }
        }
    }
}
