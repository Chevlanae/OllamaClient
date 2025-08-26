using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.ViewModels;

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

            CenterInnerGrid.DataContext = _ToolViewModel;
        }


        base.OnNavigatedTo(e);


    }

    private void SubmitFunctionButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_ToolViewModel is not null)
        {
            string text = JSCodeEditorControl.Editor.GetText(long.MaxValue);
        }
    }

    private void AddNewParameterButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (_ToolViewModel is not null && _ToolViewModel.FunctionParametersViewModel is not null)
        {
            _ToolViewModel.FunctionParametersViewModel.AddProperty("test", new()
            {
                Type = "string",
                Description = "test"
            });
        }
    }
}
