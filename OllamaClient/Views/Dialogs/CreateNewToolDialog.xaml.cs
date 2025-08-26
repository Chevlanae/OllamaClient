using Microsoft.UI.Xaml.Controls;
using OllamaClient.Models;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CreateNewToolDialog : Page
{
    public string ToolName
    {
        get => ToolNameTextBox.Text;
    }

    public string? ToolType
    {
        get => ToolTypeComboBox.SelectedItem as string;
    }

    public CreateNewToolDialog()
    {
        InitializeComponent();

        ToolTypeComboBox.ItemsSource = Enum.GetNames(typeof(Tool.ToolType));
    }
}
