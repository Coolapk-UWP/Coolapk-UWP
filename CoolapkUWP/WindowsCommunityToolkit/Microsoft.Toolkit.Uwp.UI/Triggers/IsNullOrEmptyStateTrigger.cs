// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.Helpers;
using System.Collections;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Uwp.UI.Triggers
{
    /// <summary>
    /// Enables a state if an Object is <c>null</c> or a String/IEnumerable is empty
    /// </summary>
    public class IsNullOrEmptyStateTrigger : StateTriggerBase
    {
        /// <summary>
        /// Gets or sets the value used to check for <c>null</c> or empty.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Value"/> DependencyProperty
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(object), typeof(IsNullOrEmptyStateTrigger), new PropertyMetadata(true, OnValuePropertyChanged));

        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IsNullOrEmptyStateTrigger obj = (IsNullOrEmptyStateTrigger)d;
            object val = e.NewValue;

            obj.SetActive(IsNullOrEmpty(val));

            if (val == null)
            {
                return;
            }

            // Try to listen for various notification events
            // Starting with INorifyCollectionChanged
            if (val is INotifyCollectionChanged valNotifyCollection)
            {
                WeakEventListener<IsNullOrEmptyStateTrigger, object, NotifyCollectionChangedEventArgs> weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, NotifyCollectionChangedEventArgs>(obj)
                {
                    OnEventAction = (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                    OnDetachAction = (weakEventListener) => valNotifyCollection.CollectionChanged -= weakEventListener.OnEvent
                };

                valNotifyCollection.CollectionChanged += weakEvent.OnEvent;
                return;
            }

            // Not INotifyCollectionChanged, try IObservableVector
            if (val is IObservableVector<object> valObservableVector)
            {
                WeakEventListener<IsNullOrEmptyStateTrigger, object, IVectorChangedEventArgs> weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, IVectorChangedEventArgs>(obj)
                {
                    OnEventAction = (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                    OnDetachAction = (weakEventListener) => valObservableVector.VectorChanged -= weakEventListener.OnEvent
                };

                valObservableVector.VectorChanged += weakEvent.OnEvent;
                return;
            }

            // Not INotifyCollectionChanged, try IObservableMap
            if (val is IObservableMap<object, object> valObservableMap)
            {
                WeakEventListener<IsNullOrEmptyStateTrigger, object, IMapChangedEventArgs<object>> weakEvent = new WeakEventListener<IsNullOrEmptyStateTrigger, object, IMapChangedEventArgs<object>>(obj)
                {
                    OnEventAction = (instance, source, args) => instance.SetActive(IsNullOrEmpty(source)),
                    OnDetachAction = (weakEventListener) => valObservableMap.MapChanged -= weakEventListener.OnEvent
                };

                valObservableMap.MapChanged += weakEvent.OnEvent;
            }
        }

        private static bool IsNullOrEmpty(object val)
        {
            if (val == null)
            {
                return true;
            }

            // Object is not null, check for an empty string
            if (val is string valString)
            {
                return valString.Length == 0;
            }

            // Object is not a string, check for an empty ICollection (faster)
            if (val is ICollection valCollection)
            {
                return valCollection.Count == 0;
            }

            // Object is not an ICollection, check for an empty IEnumerable
            if (val is IEnumerable valEnumerable)
            {
                foreach (object item in valEnumerable)
                {
                    // Found an item, not empty
                    return false;
                }

                return true;
            }

            // Not null and not a known type to test for emptiness
            return false;
        }
    }
}