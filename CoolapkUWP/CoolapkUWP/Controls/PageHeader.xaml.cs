using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class PageHeader : UserControl
    {
        public object Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(
                "Title",
                typeof(object),
                typeof(PageHeader),
                new PropertyMetadata(null));

        public double BackgroundColorOpacity
        {
            get => (double)GetValue(BackgroundColorOpacityProperty);
            set => SetValue(BackgroundColorOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for BackgroundColorOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundColorOpacityProperty =
            DependencyProperty.Register(
                "BackgroundColorOpacity",
                typeof(double),
                typeof(PageHeader),
                new PropertyMetadata(0.0));

        public double AcrylicOpacity
        {
            get => (double)GetValue(AcrylicOpacityProperty);
            set => SetValue(AcrylicOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for BackgroundColorOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AcrylicOpacityProperty =
            DependencyProperty.Register(
                "AcrylicOpacity",
                typeof(double),
                typeof(PageHeader),
                new PropertyMetadata(0.3));

        public double ShadowOpacity
        {
            get => (double)GetValue(ShadowOpacityProperty);
            set => SetValue(ShadowOpacityProperty, value);
        }

        // Using a DependencyProperty as the backing store for BackgroundColorOpacity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ShadowOpacityProperty =
            DependencyProperty.Register(
                "ShadowOpacity",
                typeof(double),
                typeof(PageHeader),
                new PropertyMetadata(0.0));

        public UIElement TitlePanel => PageTitle;

        public PageHeader()
        {
            InitializeComponent();
            InitializeDropShadow(ShadowHost, TitleTextBlock.GetAlphaMask());
        }

        /// <summary>
        /// This method will be called when a <see cref="ItemPage"/> gets unloaded. 
        /// Put any code in here that should be done when a <see cref="ItemPage"/> gets unloaded.
        /// </summary>
        /// <param name="sender">The sender (the ItemPage)</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> of the ItemPage that was unloaded.</param>
        public void Event_ItemPage_Unloaded(object sender, RoutedEventArgs e)
        {

        }

        private void InitializeDropShadow(UIElement shadowHost, CompositionBrush shadowTargetBrush)
        {
            Visual hostVisual = ElementCompositionPreview.GetElementVisual(shadowHost);
            Compositor compositor = hostVisual.Compositor;

            // Create a drop shadow
            DropShadow dropShadow = compositor.CreateDropShadow();
            dropShadow.Color = Color.FromArgb(102, 0, 0, 0);
            dropShadow.BlurRadius = 4.0f;
            // Associate the shape of the shadow with the shape of the target element
            dropShadow.Mask = shadowTargetBrush;

            // Create a Visual to hold the shadow
            SpriteVisual shadowVisual = compositor.CreateSpriteVisual();
            shadowVisual.Shadow = dropShadow;

            // Add the shadow as a child of the host in the visual tree
            ElementCompositionPreview.SetElementChildVisual(shadowHost, shadowVisual);

            // Make sure size of shadow host and shadow visual always stay in sync
            ExpressionAnimation bindSizeAnimation = compositor.CreateExpressionAnimation("hostVisual.Size");
            bindSizeAnimation.SetReferenceParameter("hostVisual", hostVisual);

            shadowVisual.StartAnimation("Size", bindSizeAnimation);
        }
    }
}
