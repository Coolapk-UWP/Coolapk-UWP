using CoolapkUWP.Models;
using CoolapkUWP.ViewModels.Providers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.DataSource
{
    public abstract class EntityItemSourse : DataSourceBase<Entity>
    {
        protected CoolapkListProvider Provider;

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            while (Models.Count < count)
            {
                int temp = Models.Count;
                if (Models.Count > 0) { _currentPage++; }
                await Provider.GetEntity(Models, _currentPage);
                if (Models.Count <= 0 || Models.Count <= temp) { break; }
            }
            return Models;
        }

        protected override void AddItems(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (!(item is NullEntity)) { Add(item); }
                }
            }
        }

        public virtual async Task Refresh(bool reset = false)
        {
            if (reset)
            {
                await Reset();
            }
            else
            {
                _ = await LoadMoreItemsAsync(20);
            }
        }
    }

}
