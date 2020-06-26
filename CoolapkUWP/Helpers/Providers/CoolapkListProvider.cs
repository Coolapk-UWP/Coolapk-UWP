using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.Helpers.Providers
{
    internal class CoolapkListProvider
    {
        private int page;
        private double firstItem, lastItem;
        private Func<int, int, double, double, Task<JArray>> getData;
        private readonly Func<Entity, JToken, bool> checkEqual;
        private readonly Func<JObject, IEnumerable<Entity>> getEntities;
        private readonly Func<string> getString;
        private readonly string idName;
        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();

        /// <param name="getData"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, double, double, Task<JArray>> getData,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            string idName)
            : this(
                  getData, checkEqual, getEntities,
                  () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"),
                  idName)
        {
        }

        /// <param name="getData"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, double, double, Task<JArray>> getData,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            Func<string> getString,
            string idName)
        {
            this.getData      = getData     ?? throw new ArgumentNullException(nameof(getData));
            this.checkEqual   = checkEqual  ?? throw new ArgumentNullException(nameof(checkEqual));
            this.getEntities  = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            this.getString    = getString   ?? throw new ArgumentNullException(nameof(getString));
            this.idName       = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                             : idName;
        }

        public void ChangeGetDataFunc(
            Func<int, int, double, double, Task<JArray>> getData,
            Func<Entity,bool> needDeleteJudger)
        {
            this.getData = getData ?? throw new ArgumentNullException(nameof(getData));
            var needDeleteItems = (from entity in Models
                                   where needDeleteJudger(entity)
                                   select entity).ToArray();
            foreach (var item in needDeleteItems)
            {
                Models.Remove(item);
            }
            page = 0;
        }

        public void Reset()
        {
            page = 1;
            lastItem = firstItem = 0;
            Models.Clear();
        }

        public async Task Refresh(int p = -1)
        {
            var array = await getData(p, page, firstItem, lastItem);

            if (p == -1) { page++; }

            if (array != null && array.Count > 0)
            {
                var fixedEntities = (from m in Models
                                     where m.EntityFixed
                                     select m).ToArray();
                var fixedNum = fixedEntities.Length;
                foreach (var item in fixedEntities)
                {
                    Models.Remove(item);
                }

                var needDeleteEntites = (from m in Models
                                         from b in array
                                         where checkEqual(m, b)
                                         select m).ToArray();
                foreach (var item in needDeleteEntites)
                {
                    Models.Remove(item);
                }

                for (int i = 0; i < fixedNum; i++)
                {
                    Models.Insert(i, fixedEntities[i]);
                }

                if (p == 1)
                {
                    firstItem = array.First.Value<int>(idName);
                    if (page == 1)
                    {
                        lastItem = array.Last.Value<int>(idName);
                    }

                    int modelIndex = 0;

                    for (int i = 0; i < array.Count; i++)
                    {
                        var entities = getEntities((JObject)array[i]);
                        if (entities == null) { continue; }

                        foreach (var item in entities)
                        {
                            Models.Insert(modelIndex + fixedNum, item);
                            modelIndex++;
                        }
                    }
                }
                else
                {
                    if (firstItem == 0)
                    {
                        firstItem = array.First.Value<int>(idName);
                    }
                    lastItem = array.Last.Value<int>(idName);

                    foreach (JObject item in array)
                    {
                        var entities = getEntities(item);
                        if (entities == null) { continue; }
                        foreach (var i in entities)
                        {
                            Models.Add(i);
                        }
                    }
                }
            }
            else if (p == -1)
            {
                page--;
                UIHelper.ShowMessage(getString());
            }
        }
    }
}