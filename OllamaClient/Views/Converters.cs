using Microsoft.UI.Xaml.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OllamaClient.ViewModels.SettingsViewModel.TimeoutValue;

namespace OllamaClient.Views
{
    public class IntervalOptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case "Seconds":
                    return IntervalOption.Seconds;
                case "Minutes":
                    return IntervalOption.Minutes;
                case "Hours":
                    return IntervalOption.Hours;
                default:
                    return IntervalOption.Seconds;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            switch (value)
            {
                case IntervalOption.Seconds:
                    return "Seconds";
                case IntervalOption.Minutes:
                    return "Minutes";
                case IntervalOption.Hours:
                    return "Hours";
                default:
                    return "Seconds";
            }
        }
    }
}
