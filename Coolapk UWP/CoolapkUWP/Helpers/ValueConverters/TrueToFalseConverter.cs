using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.ValueConverters
{
    public class TrueToFalseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) => Convert(value, parameter);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => Convert(value, parameter);

        private static object Convert(object value, object parameter)
        {
            if (parameter == null) { return !(bool)value; }
            else
            {
                switch (parameter)
                {
                    case "ScrollMode":
                        return (ScrollMode)value == ScrollMode.Disabled ? ScrollMode.Auto : ScrollMode.Disabled;
                    case "Visibility":
                        return (Visibility)value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    default:
                        return null;
                }
            }
        }
    }
}