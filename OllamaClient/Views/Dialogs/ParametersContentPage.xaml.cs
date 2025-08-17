using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using OllamaClient.Json;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ParametersContentPage : Page
    {
        private CreateModelDialog.InputResults Results { get; set; }

        public ParametersContentPage()
        {
            InitializeComponent();

            foreach (ModelParameterKey value in Enum.GetValues<ModelParameterKey>())
            {
                MenuFlyoutItem item = new();
                item.Text = value.ToString();
                item.Click += Item_Click;
                AddButtonMenuFlyout.Items.Add(item);
            }
        }

        private void Item_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && Enum.TryParse(item.Text, out ModelParameterKey key))
            {
                Results.Parameters?.Add(new(key, ""));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is CreateModelDialog.DialogArgs args)
            {
                Results = args.Results;

                if (Results.Parameters is null)
                {
                    Results.Parameters = new();
                }

                ParametersListView.ItemsSource = Results.Parameters;
            }

            base.OnNavigatedTo(e);
        }

        private void ClearParametersButton_Click(object sender, RoutedEventArgs e)
        {
            Results.Parameters?.Clear();
        }
    }
}
