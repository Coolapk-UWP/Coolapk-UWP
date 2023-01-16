using Windows.UI.Xaml;

namespace CoolapkUWP.Controls
{
    public class WebViewContentControlTemplateSettings : DependencyObject
    {
        public WebViewContentControlTemplateSettings()
        {
        }

        public static readonly DependencyProperty OuterCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(OuterCornerRadius),
                typeof(CornerRadius),
                typeof(WebViewContentControl),
                null);

        public CornerRadius OuterCornerRadius
        {
            get => (CornerRadius)GetValue(OuterCornerRadiusProperty);
            set => SetValue(OuterCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty InnerCornerRadiusProperty =
            DependencyProperty.Register(
                nameof(InnerCornerRadius),
                typeof(CornerRadius),
                typeof(WebViewContentControl),
                null);

        public CornerRadius InnerCornerRadius
        {
            get => (CornerRadius)GetValue(InnerCornerRadiusProperty);
            set => SetValue(InnerCornerRadiusProperty, value);
        }
    }
}
