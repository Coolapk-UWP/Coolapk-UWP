using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace CoolapkUWP.Controls
{
    public class Slot : Panel
    {
        private UIElement RootElement;

        public bool IsStretch
        {
            get => (bool)GetValue(IsStretchProperty);
            set => SetValue(IsStretchProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsStretch"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsStretchProperty =
            DependencyProperty.Register(
                nameof(IsStretch),
                typeof(bool),
                typeof(Slot),
                new PropertyMetadata(true, OnLayoutPropertyChanged));

        /// <summary>
        /// Gets or sets a value that indicates the dimension by which child elements are
        /// stacked.
        /// </summary>
        /// <returns>The Orientation of child content.</returns>
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(Slot),
                new PropertyMetadata(Orientation.Vertical, OnLayoutPropertyChanged));

        public UIElement LastControl
        {
            get => (UIElement)GetValue(LastControlProperty);
            set => SetValue(LastControlProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="LastControl"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty LastControlProperty =
            DependencyProperty.Register(
                nameof(LastControl),
                typeof(UIElement),
                typeof(Slot),
                null);

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as Slot).InvalidateArrange();
            }
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            if (RootElement is null)
            {
                RootElement = FindAscendant(this) as UIElement;
                if (RootElement is null && Window.Current != null)
                {
                    RootElement = Window.Current.Content;
                }
            }

            bool isStretch = IsStretch;
            bool fHorizontal = Orientation == Orientation.Horizontal;
            UIElementCollection children = Children;
            if (isStretch)
            {
                Rect rcChild = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);
                foreach (UIElement child in children)
                {
                    child?.Arrange(rcChild);
                    if (child is FrameworkElement element)
                    {
                        element.MaxWidth = arrangeSize.Width;
                    }
                }
            }
            else
            {
                if (fHorizontal)
                {
                    Point screenCoords = LastControl != null
                        ? LastControl.TransformToVisual(RootElement).TransformPoint(new Point(LastControl.ActualSize.X, 0))
                        : TransformToVisual(RootElement).TransformPoint(new Point(0, 0));

                    double leftPadding = Math.Max(0, screenCoords.X);
                    double rightPadding = Math.Max(0, RootElement.ActualSize.X - screenCoords.X - arrangeSize.Width);

                    if (leftPadding > rightPadding)
                    {
                        double padding = leftPadding - rightPadding;
                        double width = Math.Max(0, arrangeSize.Width - padding);
                        Rect rcChild = new Rect(0, 0, width, arrangeSize.Height);
                        foreach (UIElement child in children)
                        {
                            child?.Arrange(rcChild);
                            if (child is FrameworkElement element)
                            {
                                element.MaxWidth = width;
                            }
                        }
                    }
                    else
                    {
                        double padding = rightPadding - leftPadding;
                        double width = Math.Max(0, arrangeSize.Width - padding);
                        Rect rcChild = new Rect(padding, 0, width, arrangeSize.Height);
                        foreach (UIElement child in children)
                        {
                            child?.Arrange(rcChild);
                            if (child is FrameworkElement element)
                            {
                                element.MaxWidth = width;
                            }
                        }
                    }
                }
                else
                {
                    Point screenCoords = LastControl != null
                        ? LastControl.TransformToVisual(RootElement).TransformPoint(new Point(0, LastControl.ActualSize.Y))
                        : TransformToVisual(RootElement).TransformPoint(new Point(0, 0));

                    double topPadding = Math.Max(0, screenCoords.Y);
                    double buttonPadding = Math.Max(0, RootElement.ActualSize.Y - screenCoords.Y - arrangeSize.Height);

                    if (topPadding > buttonPadding)
                    {
                        double padding = topPadding - buttonPadding;
                        double height = Math.Max(0, arrangeSize.Height - padding);
                        Rect rcChild = new Rect(0, 0, arrangeSize.Width, height);
                        foreach (UIElement child in children)
                        {
                            child?.Arrange(rcChild);
                            if (child is FrameworkElement element)
                            {
                                element.MaxHeight = height;
                            }
                        }
                    }
                    else
                    {
                        double padding = buttonPadding - topPadding;
                        double height = Math.Max(0, arrangeSize.Height - padding);
                        Rect rcChild = new Rect(0, padding, arrangeSize.Width, height);
                        foreach (UIElement child in children)
                        {
                            child?.Arrange(rcChild);
                            if (child is FrameworkElement element)
                            {
                                element.MaxHeight = height;
                            }
                        }
                    }
                }
            }
            return arrangeSize;
        }

        private static DependencyObject FindAscendant(DependencyObject element)
        {
            DependencyObject result = null;
            while (true)
            {
                DependencyObject parent = VisualTreeHelper.GetParent(element);
                if (parent == null)
                {
                    return result;
                }
                result = element = parent;
            }
        }
    }
}
