using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CoolapkUWP.Controls
{
    class GridPanel : Panel
    {
        public double DesiredColumnWidth
        {
            get => (double)GetValue(DesiredColumnWidthProperty);
            set => SetValue(DesiredColumnWidthProperty, value);
        }
        public static readonly DependencyProperty DesiredColumnWidthProperty = DependencyProperty.Register("DesiredColumnWidth", typeof(double), typeof(GridPanel), new PropertyMetadata(384.0, RequestArrange));

        public bool CubeInSameHeight
        {
            get => (bool)GetValue(CubeInSameHeightProperty);
            set => SetValue(CubeInSameHeightProperty, value);
        }
        public static readonly DependencyProperty CubeInSameHeightProperty = DependencyProperty.Register("InSameHeight", typeof(bool), typeof(GridPanel), new PropertyMetadata(true, RequestArrange));

        private static void RequestArrange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GridPanel).InvalidateMeasure();
            (d as GridPanel).InvalidateArrange();
        }

        int StackCount = 1;
        protected override Size MeasureOverride(Size availableSize)
        {
            StackCount = (int)(availableSize.Width / DesiredColumnWidth);
            if (StackCount == 0) StackCount = 1;
            Size requestSize = new Size { Width = availableSize.Width };
            List<double> offsetY = new double[StackCount].ToList();
            double offsetY2 = 0;
            if (CubeInSameHeight)
            {
                if (Children.Count > 0)
                {
                    for (int i = 0; i < (int)Math.Ceiling((double)Children.Count / StackCount); i++)
                    {
                        double height = 0;
                        for (int j = 0; j < StackCount; j++)
                        {
                            Children[i * StackCount + j]?.Measure(new Size(availableSize.Width / StackCount, double.PositiveInfinity));
                            if (Children[i * StackCount + j]?.DesiredSize.Height > height)
                                height = Children[i * StackCount + j].DesiredSize.Height;
                        }
                        offsetY2 += height;
                    }
                }
            }
            else
            {
                foreach (var item in Children)
                {
                    if (item is ListViewItem l && l.Content is ViewModels.IndexPageViewModel m && m.entityTemplate == "iconTabLinkGridCard")
                    {
                        int maxIndex = offsetY.IndexOf(offsetY.Max());
                        item.Measure(new Size(availableSize.Width, double.PositiveInfinity));
                        var itemRequestSize = item.DesiredSize;
                        offsetY[maxIndex] += itemRequestSize.Height;
                        for (int i = 0; i < StackCount; i++)
                            offsetY[i] = offsetY[maxIndex];
                    }
                    else
                    {
                        int minIndex = offsetY.IndexOf(offsetY.Min());
                        item.Measure(new Size(availableSize.Width / StackCount, double.PositiveInfinity));
                        var itemRequestSize = item.DesiredSize;
                        offsetY[minIndex] += itemRequestSize.Height;
                    }
                }
            }
            requestSize.Height = CubeInSameHeight ? offsetY2 : offsetY.Max();
            return requestSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            List<double> offsetX = new List<double>(), offsetY = new List<double>();
            double offsetY2 = 0;
            for (int i = 0; i < StackCount; i++)
            {
                offsetX.Add(i * (DesiredSize.Width / StackCount));
                offsetY.Add(0);
            }
            if (CubeInSameHeight)
            {
                if (Children.Count > 0)
                    for (int i = 0; i < (int)Math.Ceiling((double)Children.Count / StackCount); i++)
                    {
                        double height = 0;
                        for (int j = 0; j < StackCount; j++)
                            if (Children[i * StackCount + j]?.DesiredSize.Height > height)
                                height = Children[i * StackCount + j].DesiredSize.Height;
                        for (int j = 0; j < StackCount; j++)
                            Children[i * StackCount + j]?.Arrange(new Rect(offsetX[j], offsetY2, DesiredSize.Width / StackCount, height));
                        offsetY2 += height;
                    }
            }
            else foreach (var item in Children)
                    if (item is ListViewItem l && l.Content is ViewModels.IndexPageViewModel m && m.entityTemplate == "iconTabLinkGridCard")
                    {
                        int maxIndex = offsetY.IndexOf(offsetY.Max());
                        item.Arrange(new Rect(0, offsetY[maxIndex], DesiredSize.Width, item.DesiredSize.Height));
                        offsetY[maxIndex] += item.DesiredSize.Height;
                        for (int i = 0; i < StackCount; i++)
                            offsetY[i] = offsetY[maxIndex];
                    }
                    else
                    {
                        int minIndex = offsetY.IndexOf(offsetY.Min());
                        item.Arrange(new Rect(offsetX[minIndex], offsetY[minIndex], DesiredSize.Width / StackCount, item.DesiredSize.Height));
                        offsetY[minIndex] += item.DesiredSize.Height;
                    }
            return finalSize;
        }
    }
}
