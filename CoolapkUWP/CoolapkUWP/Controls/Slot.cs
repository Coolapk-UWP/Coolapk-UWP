using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    public class Slot : Panel
    {
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

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as Slot).InvalidateArrange();
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UIElementCollection children = Children;
            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) { continue; }

                child.Measure(constraint);
            }
            return constraint;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            bool isStretch = IsStretch;
            bool fHorizontal = Orientation == Orientation.Horizontal;
            UIElementCollection children = Children;
            if (isStretch)
            {
                Rect rcChild = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height);
                UIElement child = children.FirstOrDefault();
                child?.Arrange(rcChild);
            }
            else
            {
                Window window = Window.Current;
                Point screenCoords = TransformToVisual(window.Content).TransformPoint(new Point(0, 0));
                if (fHorizontal)
                {
                    double leftPadding = window.Bounds.Width - screenCoords.X - arrangeSize.Width;
                    double rightPadding = screenCoords.X;
                    if (leftPadding > rightPadding)
                    {
                        double padding = leftPadding - rightPadding;
                        Rect rcChild = new Rect(padding, 0, arrangeSize.Width - padding, arrangeSize.Height);
                        UIElement child = children.FirstOrDefault();
                        child?.Arrange(rcChild);
                    }
                    else
                    {
                        double padding = rightPadding - leftPadding;
                        Rect rcChild = new Rect(0, 0, arrangeSize.Width - padding, arrangeSize.Height);
                        UIElement child = children.FirstOrDefault();
                        child?.Arrange(rcChild);
                    }
                }
                else
                {
                    double topPadding = screenCoords.Y;
                    double buttonPadding = window.Bounds.Height - screenCoords.Y - arrangeSize.Height;
                    if (topPadding > buttonPadding)
                    {
                        double padding = topPadding - buttonPadding;
                        Rect rcChild = new Rect(0, padding, arrangeSize.Width, arrangeSize.Height - padding);
                        UIElement child = children.FirstOrDefault();
                        child?.Arrange(rcChild);
                    }
                    else
                    {
                        double padding = buttonPadding - topPadding;
                        Rect rcChild = new Rect(0, 0, arrangeSize.Width, arrangeSize.Height - padding);
                        UIElement child = children.FirstOrDefault();
                        child?.Arrange(rcChild);
                    }
                }
            }
            return arrangeSize;
        }
    }
}
