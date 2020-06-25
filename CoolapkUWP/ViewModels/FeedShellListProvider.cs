using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    internal class FeedListProvider
    {
        private int page;
        private double firstItem, lastItem;
        private readonly Func<JObject, Entity> getEntity;
        private readonly Func<Entity, JToken, bool> checkEqual;
        private readonly Func<int, int, double, double, Task<JArray>> getData;
        private readonly Func<string> getString;
        private readonly Action moveToTop;
        private readonly string idName;

        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();

        /// <param name="getData"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public FeedListProvider(Func<int, int, double, double, Task<JArray>> getData,
                                Func<Entity, JToken, bool> checkEqual,
                                Func<JObject, Entity> getEntity,
                                Func<string> getString,
                                Action moveToTop,
                                string idName)
        {
            this.getData = getData ?? throw new ArgumentNullException(nameof(getData));
            this.checkEqual = checkEqual ?? throw new ArgumentNullException(nameof(checkEqual));
            this.getEntity = getEntity ?? throw new ArgumentNullException(nameof(getEntity));
            this.getString = getString ?? throw new ArgumentNullException(nameof(getString));
            this.moveToTop = moveToTop ?? throw new ArgumentNullException(nameof(moveToTop));
            this.idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                           : idName;
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

            if (p == -1)
            {
                page++;
            }
            else if (p == 1)
            {
                moveToTop();
            }
            if (array != null && array.Count > 0)
            {
                if (p == 1)
                {
                    var temp = (from m in Models
                                from b in array
                                where checkEqual(m, b)
                                select m).ToArray();
                    firstItem = array.First.Value<int>(idName);
                    if (page == 1)
                    {
                        lastItem = array.Last.Value<int>(idName);
                    }

                    foreach (var item in temp)
                    {
                        Models.Remove(item);
                    }

                    temp = (from m in Models
                                 where m.EntityFixed
                                 select m).ToArray();

                    foreach (var item in temp)
                    {
                        Models.Remove(item);
                    }

                    for (int i = 0; i < temp.Length; i++)
                    {
                        Models.Insert(i, temp[i]);
                    }

                    for (int i = 0; i < array.Count; i++)
                    {
                        Models.Insert(i + temp.Length, getEntity((JObject)array[i]));
                    }
                }
                else
                {
                    if(firstItem == 0)
                    {
                        firstItem = array.First.Value<int>(idName);
                    }
                    lastItem = array.Last.Value<int>(idName);
                    foreach (JObject item in array)
                    {
                        Models.Add(getEntity(item));
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