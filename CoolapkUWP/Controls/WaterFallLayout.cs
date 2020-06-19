using Microsoft.UI.Xaml.Controls;
using System.Collections.Specialized;
using Windows.Foundation;

namespace CoolapkUWP.Controls
{
    class WaterFallLayout : VirtualizingLayout
    {
        protected override void InitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = new WaterFallLayoutState();
        }
        protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
        {
            context.LayoutState = null;
        }
        protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
        {
            base.OnItemsChangedCore(context, source, args);
        }
        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            return base.ArrangeOverride(context, finalSize);
        }
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            return base.MeasureOverride(context, availableSize);
        }
    }

    class WaterFallLayoutState
    {

    }
}
