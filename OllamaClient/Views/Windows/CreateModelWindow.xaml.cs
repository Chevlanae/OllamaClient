using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using OllamaClient.ViewModels;
using OllamaClient.Views.Pages;
using System.Collections.ObjectModel;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Windows
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateModelWindow : Window
    {
        public enum ClosedReason
        {
            Created,
            Canceled
        }

        public class PageArgs
        {
            public ModelSidebarViewModel? ViewModel { get; set; }
            public InputResults? Results { get; set; }
        }

        public class InputResults
        {
            public string? Name { get; set; }
            public ModelViewModel? From { get; set; }
            public string? Template { get; set; }
            public ObservableCollection<ModelParameterViewModel>? Parameters { get; set; }
            public string? System { get; set; }
        }

        public ClosedReason Reason { get; set; } = ClosedReason.Canceled;
        public InputResults Results { get; set; } = new();

        private PageArgs Arguments { get; set; }
        private int _PreviousSelectedIndex { get; set; } = 0;

        public CreateModelWindow(ModelSidebarViewModel viewModel)
        {
            InitializeComponent();

            Arguments = new()
            {
                ViewModel = viewModel,
                Results = Results,
            };

            NavigationSelectorBar.SelectedItem = NavigationSelectorBar.Items[0];

            AppWindow.Resize(new SizeInt32(700, 600));
        }


        private void NavigationSelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            SelectorBarItem selectedItem = sender.SelectedItem;
            int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
            var transitionEffect = currentSelectedIndex - _PreviousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;
            var transitionInfo = new SlideNavigationTransitionInfo() { Effect = transitionEffect };
            _PreviousSelectedIndex = currentSelectedIndex;

            switch (currentSelectedIndex)
            {
                case 0:
                    ContentFrame.Navigate(typeof(FromContentPage), Arguments, transitionInfo);
                    break;
                case 1:
                    ContentFrame.Navigate(typeof(TemplateContentPage), Arguments, transitionInfo);
                    break;
                case 2:
                    ContentFrame.Navigate(typeof(ParametersContentPage), Arguments, transitionInfo);
                    break;
                case 3:
                    ContentFrame.Navigate(typeof(SystemContentPage), Arguments, transitionInfo);
                    break;
                default:
                    ContentFrame.Navigate(typeof(FromContentPage), Arguments, transitionInfo);
                    break;
            }
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            Reason = ClosedReason.Created;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Reason = ClosedReason.Canceled;
            this.Close();
        }
    }
}
