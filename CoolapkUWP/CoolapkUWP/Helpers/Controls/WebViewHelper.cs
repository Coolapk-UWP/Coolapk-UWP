using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Helpers
{
    public static class WebViewHelper
    {
        #region IsEnable

        public static bool GetIsEnable(WebView element)
        {
            return (bool)element.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(WebView element, bool value)
        {
            element.SetValue(IsEnableProperty, value);
        }

        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached(
                "IsEnable",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsEnableChanged));

        private static void OnIsEnableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (GetIsEnable(element)) { element.NavigationCompleted += OnNavigationCompleted; }
            else { element.NavigationCompleted -= OnNavigationCompleted; }
        }

        private static void OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            Thickness margin = GetMargin(sender);
            if (margin != null) { UpdateMargin(sender, margin); }
            bool isVerticalStretch = GetIsVerticalStretch(sender);
            UpdateIsVerticalStretch(sender, isVerticalStretch);
        }

        #endregion

        #region Margin

        public static Thickness GetMargin(WebView element)
        {
            return (Thickness)element.GetValue(MarginProperty);
        }

        public static void SetMargin(WebView element, Thickness value)
        {
            element.SetValue(MarginProperty, value);
        }

        public static readonly DependencyProperty MarginProperty =
            DependencyProperty.RegisterAttached(
                "Margin",
                typeof(Thickness),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnMarginChanged));

        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (element.IsLoaded)
            {
                Thickness margin = GetMargin(element);
                UpdateMargin(element, margin);
            }
        }

        private static void UpdateMargin(WebView element, Thickness margin)
        {
            try
            {
                Thickness Margin = margin;
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginLeft = '{Margin.Left}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginTop = '{Margin.Top}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginRight = '{Margin.Right}px'" });
                _ = element.InvokeScriptAsync("eval", new[] { $"document.body.style.marginBottom = '{Margin.Bottom}px'" });
            }
            catch { }
        }

        #endregion

        #region IsVerticalStretch

        public static bool GetIsVerticalStretch(WebView element)
        {
            return (bool)element.GetValue(IsVerticalStretchProperty);
        }

        public static void SetIsVerticalStretch(WebView element, bool value)
        {
            element.SetValue(IsVerticalStretchProperty, value);
        }

        public static readonly DependencyProperty IsVerticalStretchProperty =
            DependencyProperty.RegisterAttached(
                "IsVerticalStretch",
                typeof(bool),
                typeof(WebViewHelper),
                new PropertyMetadata(null, OnIsVerticalStretchChanged));

        private static void OnIsVerticalStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebView element = (WebView)d;
            if (element.IsLoaded)
            {
                bool isVerticalStretch = GetIsVerticalStretch(element);
                UpdateIsVerticalStretch(element, isVerticalStretch);
            }
        }

        private static async void UpdateIsVerticalStretch(WebView element, bool isVerticalStretch)
        {
            try
            {
                if (isVerticalStretch)
                {
                    string heightString = await element.InvokeScriptAsync("eval", new[] { "document.body.scrollHeight.toString()" });
                    if (double.TryParse(heightString.Trim('"'), out double height))
                    {
                        element.MinHeight = height;
                    }
                }
                else { element.MinHeight = 0; }
            }
            catch { }
        }

        #endregion
    }
}
