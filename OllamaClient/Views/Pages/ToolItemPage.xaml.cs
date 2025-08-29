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
    public class NavArgs(ToolViewModel viewModel)
    {
        public ToolViewModel ViewModel { get; set; } = viewModel;
    }

    private ToolViewModel? _ToolViewModel { get; set; }

    public ToolItemPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is NavArgs args)
        {
            _ToolViewModel = args.ViewModel;

            ParametersListView.ItemsSource = _ToolViewModel.Parameters.Properties;
            CenterInnerGrid.DataContext = _ToolViewModel;
        }

        base.OnNavigatedTo(e);
    }

    private void AddNewPropertyButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_ToolViewModel is not null)
        {
            _ToolViewModel.Parameters.Properties.Add(new("test", new(Models.Function.PropertyType.Object, "test")));
        }
    }

    private async void PickAFileButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if(sender is Button btn && App.Window is not null)
        {
            btn.IsEnabled = false;

            FileOpenPicker? picker = new();
            picker.ViewMode = PickerViewMode.List;
            picker.FileTypeFilter.Add(".js");

            nint windowHandle = WindowNative.GetWindowHandle(App.Window);
            InitializeWithWindow.Initialize(picker, windowHandle);
            StorageFile? file = await picker.PickSingleFileAsync();



            btn.IsEnabled = true;
        }
    }
}
