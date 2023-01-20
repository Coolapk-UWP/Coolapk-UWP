using System;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace CoolapkUWP.Helpers.Converters
{
    public class HTMLToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var result= value.ToString().CSStoString();
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
    }
}
