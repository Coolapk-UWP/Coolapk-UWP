using System;
using System.ComponentModel;
using Windows.UI.Xaml;

namespace CoolapkUWP.Controls
{
    public partial class Setting
    {
        /// <summary>
        /// Gets or sets the content of a ContentControl.
        /// </summary>
        /// <returns>An object that contains the control's content. The default is <see langword="null"/>.</returns>
        [Obsolete("Use Content instead of ActionContent.")]
        public object ActionContent => Content;

        #region Header

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(object),
                typeof(Setting),
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
                ((Setting)d).OnHeaderChanged();
            }
        }

        #endregion

        #region Description

        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(object),
                typeof(Setting),
                new PropertyMetadata(null, OnDescriptionPropertyChanged));

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [Localizable(true)]
        public object Description
        {
            get => GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        private static void OnDescriptionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((Setting)d).OnDescriptionChanged();
            }
        }

        #endregion

        #region Icon

        /// <summary>
        /// Identifies the <see cref="Icon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(
                nameof(Icon),
                typeof(object),
                typeof(Setting),
                new PropertyMetadata(null, OnIconPropertyChanged));

        /// <summary>
        /// Gets or sets the icon on the left.
        /// </summary>
        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((Setting)d).OnHeaderIconChanged();
            }
        }

        #endregion

        #region ActionIcon

        /// <summary>
        /// Identifies the <see cref="ActionIcon"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActionIconProperty =
            DependencyProperty.Register(
                nameof(ActionIcon),
                typeof(object),
                typeof(Setting),
                new PropertyMetadata(null, OnActionIconPropertyChanged));

        /// <summary>
        /// Gets or sets the icon that is shown when IsClickEnabled is set to true.
        /// </summary>
        public object ActionIcon
        {
            get => GetValue(ActionIconProperty);
            set => SetValue(ActionIconProperty, value);
        }

        private static void OnActionIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((Setting)d).OnButtonIconChanged();
            }
        }

        #endregion

        #region ActionIconToolTip

        /// <summary>
        /// Identifies the <see cref="ActionIconToolTip"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ActionIconToolTipProperty =
            DependencyProperty.Register(
                nameof(ActionIconToolTip),
                typeof(string),
                typeof(Setting),
                new PropertyMetadata("More"));

        /// <summary>
        /// Gets or sets the tooltip of the ActionIcon.
        /// </summary>
        public string ActionIconToolTip
        {
            get => (string)GetValue(ActionIconToolTipProperty);
            set => SetValue(ActionIconToolTipProperty, value);
        }

        #endregion

        #region IsClickEnabled

        /// <summary>
        /// Identifies the <see cref="IsClickEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsClickEnabledProperty =
            DependencyProperty.Register(
                nameof(IsClickEnabled),
                typeof(bool),
                typeof(Setting),
                new PropertyMetadata(false, OnIsClickEnabledPropertyChanged));

        /// <summary>
        /// Gets or sets if the card can be clicked.
        /// </summary>
        public bool IsClickEnabled
        {
            get => (bool)GetValue(IsClickEnabledProperty);
            set => SetValue(IsClickEnabledProperty, value);
        }

        private static void OnIsClickEnabledPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                ((Setting)d).OnIsClickEnabledChanged();
            }
        }

        #endregion

        #region ContentAlignment

        /// <summary>
        /// Identifies the <see cref="ContentAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentAlignmentProperty =
            DependencyProperty.Register(
                nameof(ContentAlignment),
                typeof(ContentAlignment),
                typeof(Setting),
                new PropertyMetadata(ContentAlignment.Right));

        /// <summary>
        /// Gets or sets the alignment of the Content.
        /// </summary>
        public ContentAlignment ContentAlignment
        {
            get => (ContentAlignment)GetValue(ContentAlignmentProperty);
            set => SetValue(ContentAlignmentProperty, value);
        }

        #endregion

        #region ContentCornerRadius

        /// <summary>
        /// Identifies the <see cref="ContentCornerRadius"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(Setting),
                null);

        /// <summary>
        /// Gets or sets the threshold of wrap with no icon.
        /// </summary>
        public CornerRadius ContentCornerRadius
        {
            get => (CornerRadius)GetValue(ContentCornerRadiusProperty);
            set => SetValue(ContentCornerRadiusProperty, value);
        }

        #endregion

        #region WrapThreshold

        /// <summary>
        /// Identifies the <see cref="WrapThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapThresholdProperty =
            DependencyProperty.Register(
                nameof(WrapThreshold),
                typeof(double),
                typeof(Setting),
                new PropertyMetadata(476));

        /// <summary>
        /// Gets or sets the threshold of wrap.
        /// </summary>
        public double WrapThreshold
        {
            get => (double)GetValue(WrapThresholdProperty);
            set => SetValue(WrapThresholdProperty, value);
        }

        #endregion

        #region WrapNoIconThreshold

        /// <summary>
        /// Identifies the <see cref="WrapNoIconThreshold"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WrapNoIconThresholdProperty =
            DependencyProperty.Register(
                nameof(WrapNoIconThreshold),
                typeof(double),
                typeof(Setting),
                new PropertyMetadata(286));

        /// <summary>
        /// Gets or sets the threshold of wrap with no icon.
        /// </summary>
        public double WrapNoIconThreshold
        {
            get => (double)GetValue(WrapNoIconThresholdProperty);
            set => SetValue(WrapNoIconThresholdProperty, value);
        }

        #endregion
    }

    public enum ContentAlignment
    {
        /// <summary>
        /// The Content is aligned to the right. Default state.
        /// </summary>
        Right,
        /// <summary>
        /// The Content is left-aligned while the Header, HeaderIcon and Description are collapsed. This is commonly used for Content types such as CheckBoxes, RadioButtons and custom layouts.
        /// </summary>
        Left,
        /// <summary>
        /// The Content is vertically aligned.
        /// </summary>
        Vertical
    }
}
