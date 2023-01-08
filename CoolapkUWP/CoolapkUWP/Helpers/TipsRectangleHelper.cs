using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// Based on <see cref="Material Libs" href="https://github.com/cnbluefire/MaterialLibs"./>
    /// </summary>
    public class TipsRectangleHelper : DependencyObject
    {
        private static readonly Dictionary<string, TipsRectangleServiceItem> TokenRectangles = new Dictionary<string, TipsRectangleServiceItem>();
        private static readonly Collection<WeakReference<Selector>> Selectors = new Collection<WeakReference<Selector>>();
        private static readonly Collection<WeakReference<Pivot>> Pivots = new Collection<WeakReference<Pivot>>();

        private static bool HasConnectedAnimation => ApiInformation.IsTypePresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation");
        private static bool HasConnectedAnimationConfiguration => ApiInformation.IsPropertyPresent("Windows.UI.Xaml.Media.Animation.ConnectedAnimation", "Configuration");

        public static string GetToken(FrameworkElement obj)
        {
            return (string)obj.GetValue(TokenProperty);
        }

        public static void SetToken(FrameworkElement obj, string value)
        {
            obj.SetValue(TokenProperty, value);
        }

        public static readonly DependencyProperty TokenProperty =
            DependencyProperty.RegisterAttached("Token", typeof(string), typeof(TipsRectangleHelper), new PropertyMetadata(null));

        public static bool GetIsEnable(FrameworkElement obj)
        {
            return HasConnectedAnimation && (bool)obj.GetValue(IsEnableProperty);
        }

        public static void SetIsEnable(FrameworkElement obj, bool value)
        {
            obj.SetValue(IsEnableProperty, value);
        }

        public static readonly DependencyProperty IsEnableProperty =
            DependencyProperty.RegisterAttached("IsEnable", typeof(bool), typeof(TipsRectangleHelper), new PropertyMetadata(true));

        public static TipsRectangleServiceStates GetState(FrameworkElement obj)
        {
            return (TipsRectangleServiceStates)obj.GetValue(StateProperty);
        }

        public static void SetState(FrameworkElement obj, TipsRectangleServiceStates value)
        {
            obj.SetValue(StateProperty, value);
        }

        public static readonly DependencyProperty StateProperty =
            DependencyProperty.RegisterAttached("State", typeof(TipsRectangleServiceStates), typeof(TipsRectangleHelper), new PropertyMetadata(TipsRectangleServiceStates.None, StatePropertyChanged));

        public static TipsRectangleServiceConfig GetConfig(DependencyObject obj)
        {
            return (TipsRectangleServiceConfig)obj.GetValue(ConfigProperty);
        }

        public static void SetConfig(DependencyObject obj, TipsRectangleServiceConfig value)
        {
            obj.SetValue(ConfigProperty, value);
        }

        public static readonly DependencyProperty ConfigProperty =
            DependencyProperty.RegisterAttached("Config", typeof(TipsRectangleServiceConfig), typeof(TipsRectangleHelper), new PropertyMetadata(TipsRectangleServiceConfig.Default));

        public static string GetTipTargetName(DependencyObject obj)
        {
            return (string)obj.GetValue(TipTargetNameProperty);
        }

        public static void SetTipTargetName(DependencyObject obj, string value)
        {
            obj.SetValue(TipTargetNameProperty, value);
        }

        public static readonly DependencyProperty TipTargetNameProperty =
            DependencyProperty.RegisterAttached("TipTargetName", typeof(string), typeof(TipsRectangleHelper), new PropertyMetadata(null, TipTargetNamePropertyChanged));

        private static void StatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue != (int)e.OldValue && e.NewValue != null)
            {
                TipsRectangleServiceStates state = (TipsRectangleServiceStates)e.NewValue;
                if (d is FrameworkElement ele)
                {
                    string token = GetToken(ele);
                    bool isenable = GetIsEnable(ele);
                    TipsRectangleServiceConfig config = GetConfig(ele);
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        switch (state)
                        {
                            case TipsRectangleServiceStates.None:
                                {
                                    if (TokenRectangles.ContainsKey(token))
                                    {
                                        TokenRectangles.Remove(token);
                                    }
                                }
                                break;
                            case TipsRectangleServiceStates.From:
                                {
                                    if (!TokenRectangles.ContainsKey(token))
                                    {
                                        TokenRectangles[token] = new TipsRectangleServiceItem(ele, null);
                                    }
                                    else if (TokenRectangles[token].TargetItem != null)
                                    {
                                        TryStartAnimation(token, config, ele, TokenRectangles[token].TargetItem, isenable);
                                        TokenRectangles.Remove(token);
                                    }
                                }
                                break;
                            case TipsRectangleServiceStates.To:
                                {
                                    if (!TokenRectangles.ContainsKey(token))
                                    {
                                        TokenRectangles[token] = new TipsRectangleServiceItem(null, ele);
                                    }
                                    else if (TokenRectangles[token].SourceItem != null)
                                    {
                                        TryStartAnimation(token, config, TokenRectangles[token].SourceItem, ele, isenable);
                                        TokenRectangles.Remove(token);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
        }

        private static void TipTargetNamePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != e.OldValue && e.NewValue is string TargetName)
            {
                if (d is Selector selector)
                {
                    bool IsIn = false;
                    WeakReference<Selector> weak_tmp = null;
                    foreach (WeakReference<Selector> item in Selectors)
                    {
                        if (item.TryGetTarget(out Selector tmp))
                        {
                            if (tmp == selector)
                            {
                                IsIn = true;
                                weak_tmp = item;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(TargetName))
                    {
                        if (IsIn)
                        {
                            Selectors.Remove(weak_tmp);
                            selector.SelectionChanged -= Selector_SelectionChanged;
                        }
                    }
                    else
                    {
                        if (!IsIn)
                        {
                            if (selector is ListViewBase || selector is ListBox)
                            {
                                weak_tmp = new WeakReference<Selector>(selector);
                                Selectors.Add(weak_tmp);
                                selector.SelectionChanged += Selector_SelectionChanged;
                            }
                        }
                    }
                }
                else if (d is Pivot pivot)
                {
                    bool IsIn = false;
                    WeakReference<Pivot> weak_tmp = null;
                    foreach (WeakReference<Pivot> item in Pivots)
                    {
                        if (item.TryGetTarget(out Pivot tmp))
                        {
                            if (tmp == pivot)
                            {
                                IsIn = true;
                                weak_tmp = item;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(TargetName))
                    {
                        if (IsIn)
                        {
                            Pivots.Remove(weak_tmp);
                            pivot.SelectionChanged -= Pivot_SelectionChanged;
                        }
                    }
                    else
                    {
                        if (!IsIn)
                        {
                            weak_tmp = new WeakReference<Pivot>(pivot);
                            Pivots.Add(weak_tmp);
                            pivot.SelectionChanged += Pivot_SelectionChanged;
                        }
                    }
                }
            }
        }

        private static void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Selector selector = (Selector)sender;
            if (!GetIsEnable(selector)) { return; }
            if (selector is ListViewBase listView)
            {
                if (listView.SelectionMode == ListViewSelectionMode.None || listView.SelectionMode == ListViewSelectionMode.Multiple)
                {
                    return;
                }
            }
            else if (selector is ListBox listBox)
            {
                if (listBox.SelectionMode == SelectionMode.Multiple)
                {
                    return;
                }
            }

            string name = GetTipTargetName(selector);
            string token = GetToken(selector);
            TipsRectangleServiceConfig config = GetConfig(selector);

            DependencyObject SourceItemContainer = null;
            DependencyObject TargetItemContainer = null;
            FrameworkElement SourceItemTips = null;
            FrameworkElement TargetItemTips = null;

            if (e.AddedItems.Count == 1 && e.RemovedItems.Count == 1)
            {
                object targetItem = e.AddedItems.FirstOrDefault();
                TargetItemContainer = selector.ContainerFromItem(targetItem);
                TargetItemTips = TargetItemContainer?.FindDescendant(name);

                object sourceItem = e.RemovedItems.FirstOrDefault();
                SourceItemContainer = selector.ContainerFromItem(sourceItem);
                SourceItemTips = SourceItemContainer?.FindDescendant(name);
            }
            if (SourceItemTips != null && TargetItemTips != null)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = selector.GetHashCode().ToString();
                }

                TryStartAnimation(token, config, SourceItemTips, TargetItemTips);
            }
        }

        private static void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = (Pivot)sender;
            if (!GetIsEnable(pivot)) { return; }

            string name = GetTipTargetName(pivot);
            string token = GetToken(pivot);
            TipsRectangleServiceConfig config = GetConfig(pivot);

            DependencyObject SourceItemContainer = null;
            DependencyObject TargetItemContainer = null;
            FrameworkElement SourceItemTips = null;
            FrameworkElement TargetItemTips = null;

            if (e.AddedItems.Count == 1 && e.RemovedItems.Count == 1)
            {
                object targetItem = e.AddedItems.FirstOrDefault();
                TargetItemContainer = pivot.ContainerFromItem(targetItem);
                if (TargetItemContainer == null) { return; }
                UIElementCollection targetItemHeaders = pivot?.FindDescendant<PivotHeaderPanel>().Children;
                foreach (UIElement header in targetItemHeaders)
                {
                    if (header is PivotHeaderItem item && item?.FindDescendant<TextBlock>().Text == (TargetItemContainer as PivotItem).Header.ToString())
                    {
                        TargetItemTips = item?.FindDescendant(name);
                        break;
                    }
                }

                object sourceItem = e.RemovedItems.FirstOrDefault();
                SourceItemContainer = pivot.ContainerFromItem(sourceItem);
                if (SourceItemContainer == null) { return; }
                UIElementCollection sourceItemHeaders = pivot?.FindDescendant<PivotHeaderPanel>().Children;
                foreach (UIElement header in sourceItemHeaders)
                {
                    if (header is PivotHeaderItem item && item?.FindDescendant<TextBlock>().Text == (SourceItemContainer as PivotItem).Header.ToString())
                    {
                        SourceItemTips = item?.FindDescendant(name);
                        break;
                    }
                }
            }
            if (SourceItemTips != null && TargetItemTips != null)
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    token = pivot.GetHashCode().ToString();
                }

                TryStartAnimation(token, config, SourceItemTips, TargetItemTips);
            }
        }

        private static void TryStartAnimation(string token, TipsRectangleServiceConfig config, FrameworkElement source, FrameworkElement target, bool isenable = true)
        {
            try
            {
                if (isenable && source.ActualHeight > 0 && source.ActualWidth > 0)
                {
                    ConnectedAnimationService service = ConnectedAnimationService.GetForCurrentView();
                    if (source != target)
                    {
                        service.GetAnimation(token)?.Cancel();
                        service.DefaultDuration = TimeSpan.FromSeconds(0.33d);
                        ConnectedAnimation animation = service.PrepareToAnimate(token, source);
                        if (HasConnectedAnimationConfiguration)
                        {
                            animation.Configuration = GetConfiguration(config);
                        }
                        animation.TryStart(target);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private static ConnectedAnimationConfiguration GetConfiguration(TipsRectangleServiceConfig selectedName)
        {
            switch (selectedName)
            {
                case TipsRectangleServiceConfig.Gravity:
                    return new GravityConnectedAnimationConfiguration();
                case TipsRectangleServiceConfig.Direct:
                    return new DirectConnectedAnimationConfiguration();
                case TipsRectangleServiceConfig.Basic:
                    return new BasicConnectedAnimationConfiguration();
                case TipsRectangleServiceConfig.Default:
                default:
                    return new BasicConnectedAnimationConfiguration();
            }
        }
    }

    internal class TipsRectangleServiceItem
    {
        public FrameworkElement SourceItem { get; set; }
        public FrameworkElement TargetItem { get; set; }
        public TipsRectangleServiceItem(FrameworkElement SourceItem, FrameworkElement TargetItem)
        {
            this.SourceItem = SourceItem;
            this.TargetItem = TargetItem;
        }
    }

    public enum TipsRectangleServiceStates
    {
        None, From, To
    }

    public enum TipsRectangleServiceConfig
    {
        Default, Gravity, Direct, Basic
    }
}
