using CoolapkUWP.Core.Models;
using System;
using Windows.UI.Xaml.Data;

namespace CoolapkUWP.Helpers.ValueConverters
{
    public class HasValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch ((string)parameter)
            {
                case "string": return !string.IsNullOrEmpty((string)value);
                case "entity array": return !((System.Collections.Immutable.ImmutableArray<Entity>)value).IsDefaultOrEmpty;
                default: return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null;
    }
}