using System;
using System.Collections;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.Converters
{
    public class CollectionCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int result = value is ICollection collection ? collection.Count : 0;
            return result.Convert(targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => value.Convert(targetType);
    }
}
