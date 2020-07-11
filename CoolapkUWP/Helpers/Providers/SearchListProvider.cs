using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.Helpers.Providers
{
    internal class SearchListProvider
    {
        private string lastItem;
        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();
        public int Page { get; private set; }

        private readonly Func<string, int, string, Task<JArray>> getData;
        private readonly Func<JObject, Entity> getEntity;
        private readonly string idName;

        /// <param name="getData"> keyword, page, lastItem </param>
        public SearchListProvider(
            Func<string, int, string, Task<JArray>> getData,
            Func<JObject, Entity> getEntity,
            string idName)
        {
            this.getData = getData ?? throw new ArgumentNullException(nameof(getData));
            this.getEntity = getEntity ?? throw new ArgumentNullException(nameof(getEntity));
            this.idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                       : idName;
        }

        public void Reset()
        {
            Page = 0;
            lastItem = string.Empty;
            Models.Clear();
        }

        public async Task Search(string keyWord)
        {
            var array = await getData(keyWord, ++Page, lastItem);
            if (Page == 1)
            {
                Models.Clear();
            }
            if (array.Count > 0)
            {
                lastItem = array.Last.Value<string>(idName);
                foreach (JObject item in array)
                {
                    Models.Add(getEntity(item));
                }
            }
            else
            {
                Page--;
                UIHelper.ShowMessage(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"));
            }
        }
    }
}