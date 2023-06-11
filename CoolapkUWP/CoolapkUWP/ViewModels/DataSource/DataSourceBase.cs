using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CoolapkUWP.ViewModels.DataSource
{
    /// <summary>
    /// Datasource base for Coolapk that enabled incremental loading (page based). <br/>
    /// Clone from <see cref="cnblogs UAP" href="https://github.com/MS-UAP/cnblogs-UAP"./>
    /// </summary>
    public abstract class DataSourceBase<T> : IncrementalLoadingBase<T>
    {
        public DataSourceBase() : base(Window.Current?.Dispatcher ?? CoreApplication.MainView.Dispatcher) { }

        public DataSourceBase(CoreDispatcher dispatcher) : base(dispatcher) { }

        /// <summary>
        /// The refresh will clear current items, and re-fetch from beginning, so that we will keep a correct page number.
        /// </summary>
        public virtual async Task Reset()
        {
            //reset
            _currentPage = 1;
            _hasMoreItems = true;

            Clear();
            _ = await LoadMoreItemsAsync(20);
        }

        protected DateTime _lastTime = DateTime.MinValue;

        protected virtual bool IsInTime()
        {
            TimeSpan delta = DateTime.Now - _lastTime;
            _lastTime = DateTime.Now;
            return delta.TotalMilliseconds < 500;
        }

        /// <summary>
        /// Special for WFun, as their items are paged.
        /// </summary>
        protected override async Task<IList<T>> LoadMoreItemsOverrideAsync(CancellationToken c, uint count)
        {
            if (IsInTime())
            {
                return null;
            }

            IList<T> newItems = await LoadItemsAsync(count);

            // Update page state.
            if (newItems != null)
            {
                _currentPage++;
            }

            _hasMoreItems = newItems != null && newItems.Count > 0;

            return newItems;
        }

        protected void FireErrorEvent(int code)
        {
            DataRequestError?.Invoke(code);
        }

        public event EventHandler<int> DataRequestError;

        protected override bool HasMoreItemsOverride() => _hasMoreItems;

        protected abstract Task<IList<T>> LoadItemsAsync(uint count);

        protected int _currentPage = 1;
        protected bool _hasMoreItems = true;
    }
}
