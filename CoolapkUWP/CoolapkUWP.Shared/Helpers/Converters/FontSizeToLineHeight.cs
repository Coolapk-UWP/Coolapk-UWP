using System;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    internal class FontSizeToLineHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => value is double size ? size * 4 / 3 : value;

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value is double size ? size * 3 / 4 : value;
    }
}
