using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
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

        private double _topheight;
        private CompositionPropertySet _propSet;
        private ScrollProgressProvider _progressProvider;
        private readonly bool HasGetElementVisual =
            SettingsHelper.Get<bool>(SettingsHelper.IsUseCompositor)
            && ApiInformation.IsMethodPresent("Windows.UI.Xaml.Hosting.ElementCompositionPreview", "GetElementVisual");

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
                    RegisterScrollViewer();
                }
            }
            if (_listViewHeader != null)
            {
                _listViewHeader.Loaded += ListViewHeader_Loaded;
                _listViewHeader.Unloaded += ListViewHeader_Unloaded;
            }
            base.OnApplyTemplate();
        }

        private void RegisterScrollViewer()
        {
            TranslateTransform transform = new TranslateTransform();
            BindingOperations.SetBinding(transform, TranslateTransform.YProperty, new Binding
            {
                Source = _scrollViewer,
                Mode = BindingMode.OneWay,
                Converter = new VerticalOffsetConverter(this),
                Path = new PropertyPath(nameof(_scrollViewer.VerticalOffset))
            });
            _listViewHeader.RenderTransform = transform;
        }

        private void ProgressProvider_ProgressChanged(object sender, double args)
        {
            _ = args == 1 || _progressProvider.Threshold == 0
                ? VisualStateManager.GoToState(this, "OnThreshold", true)
                : VisualStateManager.GoToState(this, "BeforeThreshold", true);
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
            if (items == null) { return; }
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
            if (HasGetElementVisual)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }
                _progressProvider.Threshold = _topheight;
                _propSet = _propSet ?? Window.Current.Compositor.CreatePropertySet();
                _propSet.InsertScalar("height", (float)_topheight);
            }
            _ = _scrollViewer.VerticalOffset >= _topheight || _topheight == 0
                ? VisualStateManager.GoToState(this, "OnThreshold", true)
                : VisualStateManager.GoToState(this, "BeforeThreshold", true);
        }

        private void ListViewHeader_Loaded(object sender, RoutedEventArgs e)
        {
            Grid ListViewHeader = sender as Grid;
            Canvas.SetZIndex(ItemsPanelRoot, -1);

            if (HasGetElementVisual)
            {
                if (_progressProvider == null)
                {
                    _progressProvider = new ScrollProgressProvider();
                    _progressProvider.ProgressChanged += ProgressProvider_ProgressChanged;
                    _progressProvider.ScrollViewer = _scrollViewer;
                }

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

        private void ListViewHeader_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_progressProvider != null)
            {
                _progressProvider.ProgressChanged -= ProgressProvider_ProgressChanged;
                _progressProvider = null;
            }
        }

        public class VerticalOffsetConverter : IValueConverter
        {
            public ShyHeaderListView ShyHeaderListView { get; private set; }

            public VerticalOffsetConverter(ShyHeaderListView shyHeaderListView) => ShyHeaderListView = shyHeaderListView;

            public object Convert(object value, Type targetType, object parameter, string language)
            {
                double offset = System.Convert.ToDouble(value);
                UpdateVisualState(offset);
                double result = offset < ShyHeaderListView._topheight ? 0 : -ShyHeaderListView._topheight + offset;
                return ConverterTools.Convert(result, targetType);
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                double offset = System.Convert.ToDouble(value);
                double result = offset + ShyHeaderListView._topheight;
                return ConverterTools.Convert(result, targetType);
            }

            private void UpdateVisualState(double offset)
            {
                _ = offset >= ShyHeaderListView._topheight || ShyHeaderListView._topheight == 0
                    ? VisualStateManager.GoToState(ShyHeaderListView, "OnThreshold", true)
                    : VisualStateManager.GoToState(ShyHeaderListView, "BeforeThreshold", true);
            }
        }
    }

    public class ShyHeaderItem : DependencyObject
    {
        public static readonly DependencyProperty TagProperty =
            DependencyProperty.Register(
                nameof(Tag),
                typeof(object),
                typeof(ShyHeaderItem),
                null);

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(ShyHeaderItem),
                null);

        public static readonly DependencyProperty ItemSourceProperty =
            DependencyProperty.Register(
                nameof(ItemSource),
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
