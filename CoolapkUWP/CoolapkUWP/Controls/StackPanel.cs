using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// Arranges child elements into a single line that can be oriented horizontally
    /// or vertically.
    /// </summary>
    public class StackPanel : Panel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StackPanel"/> class.
        /// </summary>
        public StackPanel() { }

        /// <summary>
        /// Gets or sets the distance between the border and its child object.
        /// </summary>
        /// <returns>The dimensions of the space between the border and its child as a <see cref="Thickness"/> value.
        /// <see cref="Thickness"/> is a structure that stores dimension values using pixel measures.</returns>
        public Thickness Padding
        {
            get => (Thickness)GetValue(PaddingProperty);
            set => SetValue(PaddingProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Padding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PaddingProperty =
            DependencyProperty.Register(
                nameof(Padding),
                typeof(Thickness),
                typeof(StackPanel),
                new PropertyMetadata(null, OnLayoutPropertyChanged));

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
                typeof(StackPanel),
                new PropertyMetadata(Orientation.Vertical, OnLayoutPropertyChanged));

        /// <summary>
        /// Gets or sets a uniform distance (in pixels) between stacked items. It is applied
        /// in the direction of the StackPanel's Orientation.
        /// </summary>
        /// <returns>The uniform distance (in pixels) between stacked items.</returns>
        public double Spacing
        {
            get => (double)GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Spacing"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SpacingProperty =
            DependencyProperty.Register(
                nameof(Spacing),
                typeof(double),
                typeof(StackPanel),
                new PropertyMetadata(0.0, OnLayoutPropertyChanged));

        private static void OnLayoutPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as StackPanel).InvalidateArrange();
            }
        }

        /// <summary>
        /// Measures the child elements of a <see cref="StackPanel"/> in anticipation
        /// of arranging them during the StackPanel.ArrangeOverride(<see cref="Size"/>)
        /// pass.
        /// </summary>
        /// <param name="constraint">An upper limit <see cref="Size"/> that should not be exceeded.</param>
        /// <returns>The <see cref="Size"/> that represents the desired size of the element.</returns>
        protected override Size MeasureOverride(Size constraint)
        {
            Size stackDesiredSize = new Size();
            UIElementCollection children = Children;
            Size layoutSlotSize = constraint;
            bool fHorizontal = Orientation == Orientation.Horizontal;
            double spacing = Spacing;
            Thickness padding = Padding;
            bool hasVisibleChild = false;

            if (fHorizontal)
            {
                layoutSlotSize.Width = double.PositiveInfinity;
            }
            else
            {
                layoutSlotSize.Height = double.PositiveInfinity;
            }

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) { continue; }

                bool isVisible = child.Visibility != Visibility.Collapsed;

                if (isVisible && !hasVisibleChild)
                {
                    hasVisibleChild = true;
                }

                child.Measure(layoutSlotSize);
                Size childDesiredSize = child.DesiredSize;

                if (fHorizontal)
                {
                    stackDesiredSize.Width += (isVisible ? spacing : 0) + childDesiredSize.Width;
                    stackDesiredSize.Height = Math.Max(stackDesiredSize.Height, childDesiredSize.Height);
                }
                else
                {
                    stackDesiredSize.Width = Math.Max(stackDesiredSize.Width, childDesiredSize.Width);
                    stackDesiredSize.Height += (isVisible ? spacing : 0) + childDesiredSize.Height;
                }
            }

            if (fHorizontal)
            {
                stackDesiredSize.Width -= hasVisibleChild ? spacing : 0;
            }
            else
            {
                stackDesiredSize.Height -= hasVisibleChild ? spacing : 0;
            }

            stackDesiredSize.Width += padding.Left + padding.Right;
            stackDesiredSize.Height += padding.Top + padding.Bottom;

            return stackDesiredSize;
        }

        /// <summary>
        /// Arranges the content of a <see cref="StackPanel"/> element.
        /// </summary>
        /// <param name="arrangeSize">The <see cref="Size"/> that this element should use to arrange its child elements.</param>
        /// <returns>
        /// The <see cref="Size"/> that represents the arranged size of this <see cref="StackPanel"/>
        /// element and its child elements.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection children = Children;
            bool fHorizontal = Orientation == Orientation.Horizontal;
            Thickness padding = Padding;
            Rect rcChild = new Rect(padding.Left, padding.Top, arrangeSize.Width - padding.Left - padding.Right, arrangeSize.Height - padding.Top - padding.Bottom);
            double previousChildSize = 0.0;
            double spacing = Spacing;

            for (int i = 0, count = children.Count; i < count; ++i)
            {
                UIElement child = children[i];

                if (child == null) { continue; }

                if (fHorizontal)
                {
                    rcChild.X += previousChildSize;
                    previousChildSize = child.DesiredSize.Width;
                    rcChild.Width = previousChildSize;
                    rcChild.Height = Math.Max(arrangeSize.Height - padding.Top - padding.Bottom, child.DesiredSize.Height);
                }
                else
                {
                    rcChild.Y += previousChildSize;
                    previousChildSize = child.DesiredSize.Height;
                    rcChild.Height = previousChildSize;
                    rcChild.Width = Math.Max(arrangeSize.Width - padding.Left - padding.Right, child.DesiredSize.Width);
                }

                if (child.Visibility != Visibility.Collapsed)
                {
                    previousChildSize += spacing;
                }

                child.Arrange(rcChild);
            }
            return arrangeSize;
        }
    }
}
