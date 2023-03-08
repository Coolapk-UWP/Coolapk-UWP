using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    [TemplatePart(Name = PartOuterBorder, Type = typeof(Border))]
    [TemplatePart(Name = PartInnerBorder, Type = typeof(Border))]
    public class WebViewContentControl : ContentControl
    {
        private const string PartOuterBorder = "OuterBorder";
        private const string PartInnerBorder = "InnerBorder";
        private Border _outerBorder;
        private Border _innerBorder;

        public WebViewContentControl() => DefaultStyleKey = typeof(WebViewContentControl);

        #region IsWebView

        public static readonly DependencyProperty IsWebViewProperty =
            DependencyProperty.Register(
                nameof(IsWebView),
                typeof(bool),
                typeof(WebViewContentControl),
                new PropertyMetadata(false, OnIsWebViewPropertyChanged));

        public bool IsWebView
        {
            get => (bool)GetValue(IsWebViewProperty);
            set => SetValue(IsWebViewProperty, value);
        }

        private static void OnIsWebViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as WebViewContentControl).UpdateCornerRadius();
            }
        }

        #endregion

        #region CornerRadius

        public static readonly DependencyProperty ContentCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(ContentCornerRadius),
                typeof(CornerRadius),
                typeof(WebViewContentControl),
                new PropertyMetadata(null, OnCornerRadiusPropertyChanged));

        public CornerRadius ContentCornerRadius
        {
            get => (CornerRadius)GetValue(ContentCornerRadiusProperty);
            set => SetValue(ContentCornerRadiusProperty, value);
        }

        private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as WebViewContentControl).UpdateCornerRadius();
            }
        }

        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _outerBorder = (Border)GetTemplateChild(PartOuterBorder);
            _innerBorder = (Border)GetTemplateChild(PartInnerBorder);

            UpdateCornerRadius();
        }

        private void UpdateCornerRadius()
        {
            if (_innerBorder != null && _outerBorder != null)
            {
                if (IsWebView)
                {
                    _innerBorder.CornerRadius = new CornerRadius(0);
                    _outerBorder.CornerRadius = ContentCornerRadius;
                }
                else
                {
                    _outerBorder.CornerRadius = new CornerRadius(0);
                    _innerBorder.CornerRadius = ContentCornerRadius;
                }
            }
        }
    }
}
