using System.ComponentModel;
using Windows.UI.Xaml;
using static QRCoder.QRCodeGenerator;

namespace CoolapkUWP.Controls
{
    public partial class QRCode
    {
        #region Content

        /// <summary>
        /// Identifies the <see cref="Content"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(
                nameof(Content),
                typeof(object),
                typeof(QRCode),
                new PropertyMetadata(null, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets the payload which shall be encoded in the QR code.
        /// </summary>
        [Localizable(true)]
        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        #endregion

        #region ECCLevel

        /// <summary>
        /// Identifies the <see cref="ECCLevel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ECCLevelProperty =
            DependencyProperty.Register(
                nameof(ECCLevel),
                typeof(ECCLevel),
                typeof(QRCode),
                new PropertyMetadata(ECCLevel.H, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets the level of error correction data.
        /// </summary>
        public ECCLevel ECCLevel
        {
            get => (ECCLevel)GetValue(ECCLevelProperty);
            set => SetValue(ECCLevelProperty, value);
        }

        #endregion

        #region IsForceUTF8

        /// <summary>
        /// Identifies the <see cref="IsForceUTF8"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsForceUTF8Property =
            DependencyProperty.Register(
                nameof(IsForceUTF8),
                typeof(bool),
                typeof(QRCode),
                new PropertyMetadata(false, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets whether the generator be forced to work in UTF-8 mode.
        /// </summary>
        public bool IsForceUTF8
        {
            get => (bool)GetValue(IsForceUTF8Property);
            set => SetValue(IsForceUTF8Property, value);
        }

        #endregion

        #region IsUTF8BOM

        /// <summary>
        /// Identifies the <see cref="IsUTF8BOM"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsUTF8BOMProperty =
            DependencyProperty.Register(
                nameof(IsUTF8BOM),
                typeof(bool),
                typeof(QRCode),
                new PropertyMetadata(false, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets whether the byte-order-mark should be used.
        /// </summary>
        public bool IsUTF8BOM
        {
            get => (bool)GetValue(IsUTF8BOMProperty);
            set => SetValue(IsUTF8BOMProperty, value);
        }

        #endregion

        #region EciMode

        /// <summary>
        /// Identifies the <see cref="EciMode"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EciModeProperty =
            DependencyProperty.Register(
                nameof(EciMode),
                typeof(EciMode),
                typeof(QRCode),
                new PropertyMetadata(EciMode.Default, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets which ECI mode shall be used.
        /// </summary>
        public EciMode EciMode
        {
            get => (EciMode)GetValue(EciModeProperty);
            set => SetValue(EciModeProperty, value);
        }

        #endregion

        #region RequestedVersion

        /// <summary>
        /// Identifies the <see cref="RequestedVersion"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty RequestedVersionProperty =
            DependencyProperty.Register(
                nameof(RequestedVersion),
                typeof(int),
                typeof(QRCode),
                new PropertyMetadata(-1, OnContentPropertyChanged));

        /// <summary>
        /// Gets or sets the fixed QR code target version.
        /// </summary>
        public int RequestedVersion
        {
            get => (int)GetValue(RequestedVersionProperty);
            set => SetValue(RequestedVersionProperty, value);
        }

        #endregion

        #region TemplateSettings

        /// <summary>
        /// Identifies the <see cref="TemplateSettings"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(QRCodeTemplateSettings),
                typeof(QRCode),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the TemplateSettings.
        /// </summary>
        public QRCodeTemplateSettings TemplateSettings
        {
            get => (QRCodeTemplateSettings)GetValue(TemplateSettingsProperty);
            set => SetValue(TemplateSettingsProperty, value);
        }

        #endregion

        #region CornerRadius

        /// <summary>
        /// Identifies the <see cref="ContentCornerRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(QRCode),
                null);

        /// <summary>
        /// Gets or sets the payload which shall be encoded in the QR code.
        /// </summary>
        public CornerRadius ContentCornerRadius
        {
            get => (CornerRadius)GetValue(ContentCornerRadiusProperty);
            set => SetValue(ContentCornerRadiusProperty, value);
        }

        #endregion

        private static void OnContentPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((QRCode)d).OnContentChanged();
            }
        }
    }
}
