using Microsoft.Toolkit.Uwp.UI.Converters;

namespace CoolapkUWP.Helpers.Converters
{
    /// <summary>
    /// This class converts a collection size to boolean.
    /// </summary>
    public class CollectionToBoolConverter : EmptyCollectionToObjectConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionToBoolConverter"/> class.
        /// </summary>
        public CollectionToBoolConverter()
        {
            NotEmptyValue = true;
            EmptyValue = false;
        }
    }
}
