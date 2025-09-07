using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Pages;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class ToolItemPage : Page
{
    public class NavArgs(ToolViewModel viewModel, ToolSidebarViewModel sidebarViewModel)
    {
        public ToolViewModel ViewModel { get; set; } = viewModel;
        public ToolSidebarViewModel SidebarViewModel { get; set; } = sidebarViewModel;
    }

    private ToolViewModel? _ToolViewModel { get; set; }
    private ToolSidebarViewModel? _ToolSidebarViewModel { get; set; }

    public ToolItemPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is NavArgs args)
        {
            _ToolViewModel = args.ViewModel;
            _ToolSidebarViewModel = args.SidebarViewModel;

            CenterInnerGrid.DataContext = _ToolViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private async void PickAFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if(sender is Button btn && App.Window is not null && _ToolViewModel is not null && _ToolSidebarViewModel is not null)
        {
            btn.IsEnabled = false;

            FileOpenPicker? picker = new();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".js");

            nint windowHandle = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(picker, windowHandle);
            StorageFile? file = await picker.PickSingleFileAsync();

            _ToolSidebarViewModel.ProcessJsFile(file);

            btn.IsEnabled = true;
        }
    }
}
