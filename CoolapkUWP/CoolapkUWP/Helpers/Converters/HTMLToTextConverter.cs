using System;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public class HTMLToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = value.ToString().CSStoString();
            return result.Convert(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value.Convert(targetType);
    }
}
