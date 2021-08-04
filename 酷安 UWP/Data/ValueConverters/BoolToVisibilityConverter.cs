using CoolapkUWP.Control.ViewModels;
using System;
using System.Collections.Immutable;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Data.ValueConverters
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "string": return !string.IsNullOrEmpty((string)value) ? Visibility.Visible : Visibility.Collapsed; ;
                case "entity array": return !((ImmutableArray<Entity>)value).IsDefaultOrEmpty ? Visibility.Visible : Visibility.Collapsed; ;
                default: return value is bool ? (bool)value ? Visibility.Visible : Visibility.Collapsed : value != null ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => (Visibility)value == Visibility.Visible;
    }
}
