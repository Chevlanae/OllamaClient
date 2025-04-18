using Microsoft.UI.Xaml.Data;
using OllamaClient.Models;
using OllamaClient.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OllamaClient.Helpers
{
    public partial class ChatItemTimestampConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DateTime timestamp)
            {
                return timestamp.ToLocalTime().ToShortDateString() + " " + timestamp.ToLocalTime().ToShortTimeString();
            }
            else return "";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemTextBoxBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChatItem item && item.Role == Enum.GetName(Role.assistant))
            {
                return "Transparent";
            }
            else return "DimGray";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ChatItem item && item.Role == Enum.GetName(Role.assistant))
            {
                return "Left";
            }
            else return "Right";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemProgressRingVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool v && v)
            {
                return "Visible";
            }
            else return "Collapsed";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public partial class ChatItemSubjectConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is null)
            {
                return "New conversation";
            }
            else return value;
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
