using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

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

    private void ToolsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListView toolsListView && toolsListView.SelectedItem is ToolViewModel viewModel && _ToolSidebarViewModel is not null)
        {
            _ContentFrame?.Navigate(typeof(ToolItemPage), new ToolItemPage.NavArgs(viewModel, _ToolSidebarViewModel));
        }
    }

    private void ToolsSidebarPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_ToolSidebarViewModel is null && XamlRoot is not null && _ContentFrame is not null)
        {
            _ToolSidebarViewModel = new(XamlRoot, DispatcherQueue, _ContentFrame);
            ToolsListView.ItemsSource = _ToolSidebarViewModel.ToolViewModelCollection;
            _ToolSidebarViewModel.Refresh();
        }
    }

    private async void GetFile_Click(object sender, RoutedEventArgs e)
    {
        if(_ToolSidebarViewModel is not null && App.Window is not null)
        {
            var handle = WindowNative.GetWindowHandle(App.Window);

            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            picker.FileTypeFilter.Add(".js");

            InitializeWithWindow.Initialize(picker, handle);

            StorageFile file = await picker.PickSingleFileAsync();

            if (file is not null) _ToolSidebarViewModel.ProcessJsFile(file);
        }
    }
}
