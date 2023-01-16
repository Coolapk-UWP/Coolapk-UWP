using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public class WebViewContentControl : ContentControl
    {
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

        public static new readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(
                nameof(CornerRadius),
                typeof(CornerRadius),
                typeof(WebViewContentControl),
                new PropertyMetadata(null, OnIsWebViewPropertyChanged));

        public new CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly DependencyProperty TemplateSettingsProperty =
            DependencyProperty.Register(
                nameof(TemplateSettings),
                typeof(WebViewContentControlTemplateSettings),
                typeof(WebViewContentControl),
                new PropertyMetadata(null, OnCornerRadiusPropertyChanged));

        private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as WebViewContentControl).UpdateCornerRadius();
            }
        }

        public WebViewContentControlTemplateSettings TemplateSettings
        {
            get => (WebViewContentControlTemplateSettings)GetValue(TemplateSettingsProperty);
            private set => SetValue(TemplateSettingsProperty, value);
        }

        private static void OnIsWebViewPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as WebViewContentControl).UpdateCornerRadius();
            }
        }

        public WebViewContentControl()
        {
            DefaultStyleKey = typeof(WebViewContentControl);
            SetValue(TemplateSettingsProperty, new WebViewContentControlTemplateSettings());
        }

        private void UpdateCornerRadius()
        {
            if (IsWebView)
            {
                TemplateSettings.OuterCornerRadius = new CornerRadius(0);
                TemplateSettings.InnerCornerRadius = CornerRadius;
            }
            else
            {
                TemplateSettings.OuterCornerRadius = CornerRadius;
                TemplateSettings.InnerCornerRadius = new CornerRadius(0);
            }
        }
    }
}
