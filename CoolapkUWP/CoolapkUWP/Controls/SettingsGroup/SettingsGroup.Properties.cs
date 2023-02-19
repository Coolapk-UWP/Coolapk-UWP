using Windows.UI.Xaml;
using System.ComponentModel;

namespace CoolapkUWP.Controls
{
    public partial class SettingsGroup
    {
        #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(SettingsGroup),
                new PropertyMetadata(null, OnHeaderPropertyChanged));

        /// <summary>
        /// Gets or sets the Header.
        /// </summary>
        [Localizable(true)]
        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((SettingsGroup)d).OnHeaderChanged();
            }
        }

        #endregion
    }
}
