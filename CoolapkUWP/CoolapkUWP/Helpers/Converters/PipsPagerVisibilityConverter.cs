using System;
using System.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public class PipsPagerVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool result = value is ICollection collection && collection.Count > 1;
            Visibility visibility = result ? Visibility.Visible : Visibility.Collapsed;
            return ConverterTools.Convert(visibility, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value, targetType);
    }
}
