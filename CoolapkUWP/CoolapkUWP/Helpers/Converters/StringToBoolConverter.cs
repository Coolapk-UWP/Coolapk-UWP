using Microsoft.Toolkit.Uwp.UI.Converters;

namespace CoolapkUWP.Helpers.Converters
{
    /// <summary>
    /// This class converts a string value into a Boolean value (if the value is null or empty returns a false value).
    /// </summary>
    public class StringToBoolConverter : EmptyStringToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringVisibilityConverter"/> class.
        /// </summary>
        public StringToBoolConverter()
        {
            NotEmptyValue = true;
            EmptyValue = false;
        }
    }
}
