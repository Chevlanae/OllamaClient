using System;
using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

        public sealed partial class MainWindow : Window
    {
        private ObservableCollection<string> AvailableModels { get; set; }

        public MainWindow()
        {
            AvailableModels = [];
            AvailableModels.Add("deepseek-r1:14b");
            AvailableModels.Add("deepseek-r1:32b");
            AvailableModels.Add("deepseek-r1:70b");


            InitializeComponent();

            ModelSelectorComboBox.ItemsSource = AvailableModels;
            ModelSelectorComboBox.SelectedIndex = 0;
        }

        private void ToggleSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            TopLevelSplitView.IsPaneOpen = !TopLevelSplitView.IsPaneOpen;
        }

        private void ModelSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is string model)
                {
                    Type t = typeof(ChatSessionPage);

                    ContentFrame.Navigate(t, model);
                }
            }
        }
    }
}
