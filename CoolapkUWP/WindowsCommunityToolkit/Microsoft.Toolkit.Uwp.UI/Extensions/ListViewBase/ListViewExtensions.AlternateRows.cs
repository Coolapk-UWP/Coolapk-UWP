// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Microsoft.Toolkit.Uwp.UI
{
    /// <summary>
    /// Provides attached dependency properties for the <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
    /// </summary>
    public static partial class ListViewExtensions
    {
        private static readonly Dictionary<IObservableVector<object>, Windows.UI.Xaml.Controls.ListViewBase> _itemsForList = new Dictionary<IObservableVector<object>, Windows.UI.Xaml.Controls.ListViewBase>();

        /// <summary>
        /// Attached <see cref="DependencyProperty"/> for binding a <see cref="Brush"/> as an alternate background color to a <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
        /// </summary>
        public static readonly DependencyProperty AlternateColorProperty = DependencyProperty.RegisterAttached("AlternateColor", typeof(Brush), typeof(ListViewExtensions), new PropertyMetadata(null, OnAlternateColorPropertyChanged));

        /// <summary>
        /// Attached <see cref="DependencyProperty"/> for binding a <see cref="DataTemplate"/> as an alternate template to a <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
        /// </summary>
        public static readonly DependencyProperty AlternateItemTemplateProperty = DependencyProperty.RegisterAttached("AlternateItemTemplate", typeof(DataTemplate), typeof(ListViewExtensions), new PropertyMetadata(null, OnAlternateItemTemplatePropertyChanged));

        /// <summary>
        /// Gets the alternate <see cref="Brush"/> associated with the specified <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.ListViewBase"/> to get the associated <see cref="Brush"/> from</param>
        /// <returns>The <see cref="Brush"/> associated with the <see cref="Windows.UI.Xaml.Controls.ListViewBase"/></returns>
        public static Brush GetAlternateColor(Windows.UI.Xaml.Controls.ListViewBase obj)
        {
            return (Brush)obj.GetValue(AlternateColorProperty);
        }

        /// <summary>
        /// Sets the alternate <see cref="Brush"/> associated with the specified <see cref="DependencyObject"/>
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.ListViewBase"/> to associate the <see cref="Brush"/> with</param>
        /// <param name="value">The <see cref="Brush"/> for binding to the <see cref="Windows.UI.Xaml.Controls.ListViewBase"/></param>
        public static void SetAlternateColor(Windows.UI.Xaml.Controls.ListViewBase obj, Brush value)
        {
            obj.SetValue(AlternateColorProperty, value);
        }

        /// <summary>
        /// Gets the <see cref="DataTemplate"/> associated with the specified <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.ListViewBase"/> to get the associated <see cref="DataTemplate"/> from</param>
        /// <returns>The <see cref="DataTemplate"/> associated with the <see cref="Windows.UI.Xaml.Controls.ListViewBase"/></returns>
        public static DataTemplate GetAlternateItemTemplate(Windows.UI.Xaml.Controls.ListViewBase obj)
        {
            return (DataTemplate)obj.GetValue(AlternateItemTemplateProperty);
        }

        /// <summary>
        /// Sets the <see cref="DataTemplate"/> associated with the specified <see cref="Windows.UI.Xaml.Controls.ListViewBase"/>
        /// </summary>
        /// <param name="obj">The <see cref="Windows.UI.Xaml.Controls.ListViewBase"/> to associate the <see cref="DataTemplate"/> with</param>
        /// <param name="value">The <see cref="DataTemplate"/> for binding to the <see cref="Windows.UI.Xaml.Controls.ListViewBase"/></param>
        public static void SetAlternateItemTemplate(Windows.UI.Xaml.Controls.ListViewBase obj, DataTemplate value)
        {
            obj.SetValue(AlternateItemTemplateProperty, value);
        }

        private static void OnAlternateColorPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is Windows.UI.Xaml.Controls.ListViewBase listViewBase))
            {
                return;
            }

            listViewBase.ContainerContentChanging -= ColorContainerContentChanging;
            listViewBase.Items.VectorChanged -= ColorItemsVectorChanged;
            listViewBase.Unloaded -= OnListViewBaseUnloaded;

            _itemsForList[listViewBase.Items] = listViewBase;
            if (AlternateColorProperty != null)
            {
                listViewBase.ContainerContentChanging += ColorContainerContentChanging;
                listViewBase.Items.VectorChanged += ColorItemsVectorChanged;
                listViewBase.Unloaded += OnListViewBaseUnloaded;
            }
        }

        private static void ColorContainerContentChanging(Windows.UI.Xaml.Controls.ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            Control itemContainer = args.ItemContainer;
            SetItemContainerBackground(sender, itemContainer, args.ItemIndex);
        }

        private static void OnAlternateItemTemplatePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is Windows.UI.Xaml.Controls.ListViewBase listViewBase))
            {
                return;
            }

            listViewBase.ContainerContentChanging -= ItemTemplateContainerContentChanging;
            listViewBase.Unloaded -= OnListViewBaseUnloaded;

            if (AlternateItemTemplateProperty != null)
            {
                listViewBase.ContainerContentChanging += ItemTemplateContainerContentChanging;
                listViewBase.Unloaded += OnListViewBaseUnloaded;
            }
        }

        private static void ItemTemplateContainerContentChanging(Windows.UI.Xaml.Controls.ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            SelectorItem itemContainer = args.ItemContainer;

            itemContainer.ContentTemplate = args.ItemIndex % 2 == 0 ? GetAlternateItemTemplate(sender) : sender.ItemTemplate;
        }

        private static void OnItemContainerStretchDirectionPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!(sender is Windows.UI.Xaml.Controls.ListViewBase listViewBase))
            {
                return;
            }

            listViewBase.ContainerContentChanging -= ItemContainerStretchDirectionChanging;
            listViewBase.Unloaded -= OnListViewBaseUnloaded;

            if (ItemContainerStretchDirectionProperty != null)
            {
                listViewBase.ContainerContentChanging += ItemContainerStretchDirectionChanging;
                listViewBase.Unloaded += OnListViewBaseUnloaded;
            }
        }

        private static void ItemContainerStretchDirectionChanging(Windows.UI.Xaml.Controls.ListViewBase sender, ContainerContentChangingEventArgs args)
        {
            SelectorItem itemContainer = args.ItemContainer;
            ItemContainerStretchDirection stretchDirection = GetItemContainerStretchDirection(sender);

            if (stretchDirection == ItemContainerStretchDirection.Vertical || stretchDirection == ItemContainerStretchDirection.Both)
            {
                itemContainer.VerticalContentAlignment = VerticalAlignment.Stretch;
            }

            if (stretchDirection == ItemContainerStretchDirection.Horizontal || stretchDirection == ItemContainerStretchDirection.Both)
            {
                itemContainer.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            }
        }

        private static void OnListViewBaseUnloaded(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Controls.ListViewBase listViewBase = sender as Windows.UI.Xaml.Controls.ListViewBase;
            _itemsForList.Remove(listViewBase.Items);

            listViewBase.ContainerContentChanging -= ItemContainerStretchDirectionChanging;
            listViewBase.ContainerContentChanging -= ItemTemplateContainerContentChanging;
            listViewBase.ContainerContentChanging -= ColorContainerContentChanging;
            listViewBase.Items.VectorChanged -= ColorItemsVectorChanged;
            listViewBase.Unloaded -= OnListViewBaseUnloaded;
        }

        private static void ColorItemsVectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs args)
        {
            // If the index is at the end we can ignore
            if (args.Index == (sender.Count - 1))
            {
                return;
            }

            // Only need to handle Inserted and Removed because we'll handle everything else in the
            // ColorContainerContentChanging method
            if ((args.CollectionChange == CollectionChange.ItemInserted) || (args.CollectionChange == CollectionChange.ItemRemoved))
            {
                _itemsForList.TryGetValue(sender, out Windows.UI.Xaml.Controls.ListViewBase listViewBase);
                if (listViewBase == null)
                {
                    return;
                }

                int index = (int)args.Index;
                for (int i = index; i < sender.Count; i++)
                {
                    if (listViewBase.ContainerFromIndex(i) is Control itemContainer)
                    {
                        SetItemContainerBackground(listViewBase, itemContainer, i);
                    }
                }
            }
        }

        private static void SetItemContainerBackground(Windows.UI.Xaml.Controls.ListViewBase sender, Control itemContainer, int itemIndex)
        {
            itemContainer.Background = itemIndex % 2 == 0 ? GetAlternateColor(sender) : null;
        }
    }
}