using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OllamaClient.Views.Dialogs
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateModelDialog : Page
    {

        public string? SelectedModel { get; set; }

        private int _PreviousSelectedIndex { get; set; } = 0;
        private ComboBox AvailableModelsComboBox { get; set; }

        public CreateModelDialog(CreateModelContentDialog.DialogArgs args)
        {
            InitializeComponent();

            AvailableModelsComboBox = new();
            AvailableModelsComboBox.ItemsSource = args.AvailableModels;
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
                    ContentFrame.Navigate(typeof(FromContentPage), AvailableModelsComboBox, transitionInfo);
                    break;
                case 1:
                    ContentFrame.Navigate(typeof(TemplateContentPage), transitionInfo);
                    break;
                case 2:
                    ContentFrame.Navigate(typeof(ParametersContentPage), transitionInfo);
                    break;
                case 3:
                    ContentFrame.Navigate(typeof(SystemContentPage), transitionInfo);
                    break;
                default:
                    ContentFrame.Navigate(typeof(FromContentPage), AvailableModelsComboBox, transitionInfo);
                    break;
            }
        }
    }
}
