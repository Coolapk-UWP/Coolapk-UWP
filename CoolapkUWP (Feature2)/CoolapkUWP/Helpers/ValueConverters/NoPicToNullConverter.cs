using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Data.ValueConverters
{
    public class NoPicToNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value is ImageSource Image && Image == ImageCache.NoPic ? null : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}
