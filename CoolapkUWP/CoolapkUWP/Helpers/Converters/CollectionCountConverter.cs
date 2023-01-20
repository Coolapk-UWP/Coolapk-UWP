using System;
using System.Collections;
using System.Reflection;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;

namespace CoolapkUWP.Helpers.Converters
{
    public class CollectionCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int result = value is ICollection collection ? collection.Count : 0;
            return targetType.IsInstanceOfType(result) ? result : XamlBindingHelper.ConvertValue(targetType, result);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            targetType.IsInstanceOfType(value) ? value : XamlBindingHelper.ConvertValue(targetType, value);
    }
}
