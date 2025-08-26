using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ToolsSidebarPage : Page
{
    public class NavArgs(Frame contentFrame)
    {
        public Frame ContentFrame { get; set; } = contentFrame;
    }

    private Frame? _ContentFrame { get; set; }
    private ToolSidebarViewModel? _ToolSidebarViewModel;

    public ToolsSidebarPage()
    {
        InitializeComponent();

        Loaded += ToolsSidebarPage_Loaded;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is NavArgs args)
        {
            _ContentFrame = args.ContentFrame;
        }

        base.OnNavigatedTo(e);
    }

    private void AddNewTool_Click(object sender, RoutedEventArgs e)
    {
        _ToolSidebarViewModel?.ShowCreateNewToolDialog();
    }

    private void ToolsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView toolsListView && toolsListView.SelectedItem is ToolViewModel viewModel)
        {
            _ContentFrame?.Navigate(typeof(ToolItemPage), new ToolItemPage.NavArgs(viewModel));
        }
    }

    private void ToolsSidebarPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_ToolSidebarViewModel is null && XamlRoot is not null)
        {
            _ToolSidebarViewModel = new(XamlRoot, DispatcherQueue);
            ToolsListView.ItemsSource = _ToolSidebarViewModel.ToolViewModelCollection;
            _ToolSidebarViewModel.Refresh();
        }
    }
}
