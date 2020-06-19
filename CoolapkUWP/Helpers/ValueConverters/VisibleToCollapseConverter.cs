using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.ValueConverters
{
    public class VisibleToCollapseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        public object ConvertBack(object value, Type targetType, object parameter, string language) 
            => (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
}