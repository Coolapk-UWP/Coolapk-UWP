using CoolapkUWP.Common;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkUWP.Controls
{
    [TemplatePart(Name = "TopHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "ListViewHeader", Type = typeof(Grid))]
    [TemplatePart(Name = "PivotHeader", Type = typeof(PivotHeader))]
    [TemplatePart(Name = "ScrollViewer", Type = typeof(ScrollViewer))]
    public sealed class ShyHeaderListView : ListView, IShyHeader
    {
        private Grid _topHeader;
        private Grid _listViewHeader;
        private PivotHeader _pivotHeader;
        private ScrollViewer _scrollViewer;

        private double _offset;
        private double _topheight;
        private CompositionPropertySet _propSet;
        private readonly ScrollProgressProvider _progressProvider;
        private bool HasGetElementVisual => ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementVisual");

        public static readonly DependencyProperty TopHeaderProperty =
            DependencyProperty.Register(
                nameof(TopHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty LeftHeaderProperty =
            DependencyProperty.Register(
                nameof(LeftHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty RightHeaderProperty =
            DependencyProperty.Register(
                nameof(RightHeader),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty HeaderMarginProperty =
            DependencyProperty.Register(
                nameof(HeaderMargin),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(0d));

        public static readonly DependencyProperty HeaderHeightProperty =
            DependencyProperty.Register(
                nameof(HeaderHeight),
                typeof(double),
                typeof(ShyHeaderListView),
                new PropertyMetadata(double.NaN));

        public static readonly DependencyProperty HeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(HeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty TopHeaderBackgroundProperty =
            DependencyProperty.Register(
                nameof(TopHeaderBackground),
                typeof(Brush),
                typeof(ShyHeaderListView),
                null);

        public static readonly DependencyProperty ShyHeaderItemSourceProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderItemSource),
                typeof(IList<ShyHeaderItem>),
                typeof(ShyHeaderListView),
                new PropertyMetadata(null, OnShyHeaderItemSourcePropertyChanged));

        public static readonly DependencyProperty ShyHeaderSelectedIndexProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderSelectedIndex),
                typeof(int),
                typeof(ShyHeaderListView),
                new PropertyMetadata(-1, OnShyHeaderSelectedIndexPropertyChanged));

        public static readonly DependencyProperty ShyHeaderSelectedItemProperty =
            DependencyProperty.Register(
                nameof(ShyHeaderSelectedItem),
                typeof(object),
                typeof(ShyHeaderListView),
                null);

        public event SelectionChangedEventHandler ShyHeaderSelectionChanged;

        public object TopHeader
        {
            get => GetValue(TopHeaderProperty);
            set => SetValue(TopHeaderProperty, value);
        }

        public object LeftHeader
        {
            get => GetValue(LeftHeaderProperty);
            set => SetValue(LeftHeaderProperty, value);
        }

        public object RightHeader
        {
            get => GetValue(RightHeaderProperty);
            set => SetValue(RightHeaderProperty, value);
        }

        public double HeaderMargin
        {
            get => (double)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        public double HeaderHeight
        {
            get => (double)GetValue(HeaderHeightProperty);
            set => SetValue(HeaderHeightProperty, value);
        }

        public Brush HeaderBackground
        {
            get => (Brush)GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        public Brush TopHeaderBackground
        {
            get => (Brush)GetValue(TopHeaderBackgroundProperty);
            set => SetValue(TopHeaderBackgroundProperty, value);
        }

        public IList<ShyHeaderItem> ShyHeaderItemSource
        {
            get => (IList<ShyHeaderItem>)GetValue(ShyHeaderItemSourceProperty);
            set => SetValue(ShyHeaderItemSourceProperty, value);
        }

        public int ShyHeaderSelectedIndex
        {
            get => (int)GetValue(ShyHeaderSelectedIndexProperty);
            set => SetValue(ShyHeaderSelectedIndexProperty, value);
        }

        public object ShyHeaderSelectedItem
        {
            get => GetValue(ShyHeaderSelectedItemProperty);
            set => SetValue(ShyHeaderSelectedItemProperty, value);
        }

        private static void OnShyHeaderItemSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as ShyHeaderListView).UpdateShyHeaderItem(e.NewValue as IList<ShyHeaderItem>);
            }
        }

        private static void OnShyHeaderSelectedIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue)
            {
                (d as ShyHeaderListView).UpdateShyHeaderSelectedIndex(e.NewValue as int?);
            }
        }

        public ShyHeaderListView()
        {
            DefaultStyleKey = typeof(ShyHeaderListView);
            if (HasGetElementVisual)
            {
                _progressProvider = new ScrollProgressProvider();
                ScrollViewerExtensions.SetEnableMiddleClickScrolling(this, true);
                _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
            }
        }

        protected override void OnApplyTemplate()
        {
            if (_topHeader != null)
            {
                _topHeader.SizeChanged -= TopHeader_SizeChanged;
            }
            if (_pivotHeader != null)
            {
                _pivotHeader.SelectionChanged -= PivotHeader_SelectionChanged;
            }
            if (_scrollViewer != null)
            {
                _scrollViewer.ViewChanged -= ScrollViewer_ViewChanged;
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded -= ListViewHeader_Loaded;
            }

            _topHeader = (Grid)GetTemplateChild("TopHeader");
            _listViewHeader = (Grid)GetTemplateChild("ListViewHeader");
            _pivotHeader = (PivotHeader)GetTemplateChild("PivotHeader");
            _scrollViewer = (ScrollViewer)GetTemplateChild("ScrollViewer");

            if (_topHeader != null)
            {
                _topHeader.SizeChanged += TopHeader_SizeChanged;
            }
            if (_pivotHeader != null)
            {
                try { _pivotHeader.SelectedIndex = 0; } catch { }
                _pivotHeader.SelectionChanged += PivotHeader_SelectionChanged;
                if (ShyHeaderItemSource != null)
                {
                    UpdateShyHeaderItem();
                }
            }
            if (_scrollViewer != null)
            {
                if (_progressProvider != null)
                {
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
                else
                {
                    _scrollViewer.ViewChanged += ScrollViewer_ViewChanged;
                }
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded += ListViewHeader_Loaded;
            }
            base.OnApplyTemplate();
        }

        protected override void OnItemsChanged(object e)
        {
            if (_progressProvider != null)
            {
                _scrollViewer.ChangeView(null, _progressProvider.Progress * _progressProvider.Threshold, null, true);
            }
            else
            {
                _scrollViewer.ChangeView(null, Math.Min(_offset, _topheight), null, true);
            }
            base.OnItemsChanged(e);
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            _offset = _scrollViewer.VerticalOffset;
            double Translation = _scrollViewer.VerticalOffset < _topheight ? 0 : -_topheight + _scrollViewer.VerticalOffset;
            _listViewHeader.RenderTransform = new TranslateTransform { Y = Translation };
            if (_scrollViewer.VerticalOffset >= _topheight || _topheight == 0)
            {
                VisualStateManager.GoToState(this, "OnThreshold", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "BeforeThreshold", true);
            }
        }

        private void ProgressProvider_ProgressChanged(object sender, double args)
        {
            if (args == 1 || _progressProvider.Threshold == 0)
            {
                VisualStateManager.GoToState(this, "OnThreshold", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "BeforeThreshold", true);
            }
        }

        private void PivotHeader_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ShyHeaderSelectedIndex = (sender as PivotHeader).SelectedIndex;
            IList<object> AddedItems = (from item in ShyHeaderItemSource
                                        where e.AddedItems.Contains(item.Header)
                                        select (object)item).ToList();
            IList<object> RemovedItems = (from item in ShyHeaderItemSource
                                          where e.RemovedItems.Contains(item.Header)
                                          select (object)item).ToList();
            ShyHeaderSelectionChanged?.Invoke(this, new SelectionChangedEventArgs(RemovedItems, AddedItems));
        }

        private void UpdateShyHeaderItem(IList<ShyHeaderItem> items = null)
        {
            items = items ?? ShyHeaderItemSource;
            if (_pivotHeader != null)
            {
                _pivotHeader.ItemsSource = (from item in items
                                            select item?.Header ?? string.Empty).ToArray();
            }
            if (_pivotHeader?.SelectedIndex == -1)
            {
                try { _pivotHeader.SelectedIndex = 0; } catch { }
            }
        }

        private void UpdateShyHeaderSelectedIndex(int? index = null)
        {
            index = index ?? SelectedIndex;
            if (index == -1) { return; }
            ShyHeaderSelectedItem = ShyHeaderItemSource[(int)index];
            ItemsSource = ShyHeaderItemSource[(int)index].ItemSource;
        }

        private void TopHeader_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Grid TopHeader = sender as Grid;
            _topheight = Math.Max(0, TopHeader.ActualHeight - HeaderMargin);
            if (_progressProvider != null)
            {
                _progressProvider.Threshold = _topheight;
            }
            if (HasGetElementVisual)
            {
                _propSet = _propSet ?? Window.Current.Compositor.CreatePropertySet();
                _propSet.InsertScalar("height", (float)_topheight);
            }
            if (_scrollViewer.VerticalOffset >= _topheight || _topheight == 0)
            {
                VisualStateManager.GoToState(this, "OnThreshold", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "BeforeThreshold", true);
            }
        }

        private void ListViewHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Grid ListViewHeader = sender as Grid;
            Canvas.SetZIndex(ItemsPanelRoot, -1);

            if (HasGetElementVisual)
            {
                Visual _headerVisual = ElementCompositionPreview.GetElementVisual(ListViewHeader);
                CompositionPropertySet _manipulationPropertySet = ElementCompositionPreview.GetScrollViewerManipulationPropertySet(_scrollViewer);

                _propSet = _propSet ?? Window.Current.Compositor.CreatePropertySet();
                _propSet.InsertScalar("height", (float)Math.Max(0, _topHeader.ActualHeight - HeaderMargin));

                Compositor _compositor = Window.Current.Compositor;
                ExpressionAnimation _headerAnimation = _compositor.CreateExpressionAnimation("_manipulationPropertySet.Translation.Y > -_propSet.height ? 0: -_propSet.height -_manipulationPropertySet.Translation.Y");

                _headerAnimation.SetReferenceParameter("_propSet", _propSet);
                _headerAnimation.SetReferenceParameter("_manipulationPropertySet", _manipulationPropertySet);

                _headerVisual.StartAnimation("Offset.Y", _headerAnimation);
            }
            else
            {
                _topheight = Math.Max(0, _topHeader.ActualHeight - HeaderMargin);
            }
        }
    }

    public class ShyHeaderItem : DependencyObject
    {
        public static readonly DependencyProperty TagProperty = DependencyProperty.Register(
           "Tag",
           typeof(object),
           typeof(ShyHeaderItem),
           null);

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
           "Header",
           typeof(string),
           typeof(ShyHeaderItem),
           null);

        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(
           "ItemSource",
           typeof(object),
           typeof(ShyHeaderItem),
           null);

        public object Tag
        {
            get => GetValue(TagProperty);
            set => SetValue(TagProperty, value);
        }

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public object ItemSource
        {
            get => GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }
    }
}
