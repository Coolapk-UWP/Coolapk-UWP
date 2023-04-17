using System;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace CoolapkUWP.Helpers.Converters
{
    public class FontSizeToHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 4 / 3;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            object result = System.Convert.ToDouble(value) * 3 / 4;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }
    }
}
