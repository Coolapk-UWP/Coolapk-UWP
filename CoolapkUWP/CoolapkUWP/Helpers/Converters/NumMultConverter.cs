using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace CoolapkUWP.Helpers.Converters
{
    public class NumMultConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double result = System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double result = System.Convert.ToDouble(value) / System.Convert.ToDouble(parameter);
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }
    }
}
