using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls
{
    public partial class QRCodeTemplateSettings : DependencyObject
    {
        #region GeometryGroup

        /// <summary>
        /// Identifies the <see cref="GeometryGroup"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GeometryGroupProperty =
            DependencyProperty.Register(
                nameof(GeometryGroup),
                typeof(GeometryGroup),
                typeof(QRCodeTemplateSettings),
                null);

        /// <summary>
        /// Gets or sets the GeometryGroup.
        /// </summary>
        public GeometryGroup GeometryGroup
        {
            get => (GeometryGroup)GetValue(GeometryGroupProperty);
            set => SetValue(GeometryGroupProperty, value);
        }

        #endregion
    }
}
