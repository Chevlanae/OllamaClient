using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ModelSidebarPage : Page
    {
        public class NavArgs(Frame contentFrame)
        {
            public Frame ContentFrame { get; set; } = contentFrame;
        }

        private Frame? _ContentFrame { get; set; }
        private ModelSidebarViewModel? _SidebarViewModel { get; set; }

        public ModelSidebarPage()
        {
            InitializeComponent();

            Loaded += ModelSidebarPage_Loaded;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is NavArgs args)
            {
                _ContentFrame = args.ContentFrame;
            }

            if (_SidebarViewModel is null)
            {
                SetViewModel();
            }

            _SidebarViewModel?.Refresh();

            ModelsListView.SelectedIndex = -1;

            base.OnNavigatedTo(e);
        }


        private void ModelSidebarPage_Loaded(object sender, RoutedEventArgs e)
        {
            SetViewModel();
        }

        private void ModelsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ModelsListView.SelectedItem is ModelViewModel item && _SidebarViewModel is not null)
            {
                _ContentFrame?.Navigate(typeof(ModelItemPage), new ModelItemPage.NavArgs(item, _SidebarViewModel));
            }
        }

        private void CreateModelButton_Click(object sender, RoutedEventArgs e)
        {
            _SidebarViewModel?.ShowCreateModelDialog();
        }

        private void PullModelButton_Click(object sender, RoutedEventArgs e)
        {
            _SidebarViewModel?.ShowPullModelDialog();
        }

        private void RefreshModelsButton_Click(object sender, RoutedEventArgs e)
        {
            _SidebarViewModel?.Refresh();
        }

        private void SetViewModel()
        {
            if (_SidebarViewModel is null && XamlRoot is not null)
            {
                _SidebarViewModel = new(ModelsListView, XamlRoot, DispatcherQueue);
                ModelsListView.ItemsSource = _SidebarViewModel.ModelViewModelCollection;

                _SidebarViewModel.Refresh();
            }
        }
    }
}
