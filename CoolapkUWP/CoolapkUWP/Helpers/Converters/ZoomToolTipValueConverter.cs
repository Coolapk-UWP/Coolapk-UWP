using System;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public class ZoomToolTipValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => ConverterTools.Convert($"{value}%", targetType);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => ConverterTools.Convert(value.ToString().Replace("%", string.Empty), targetType);
    }
}
