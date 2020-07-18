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
        private string firstItem, lastItem;
        private Func<int, int, string, string, Uri> _getUri;
        private readonly Func<Entity, JToken, bool> _checkEqual;
        private readonly Func<JObject, IEnumerable<Entity>> _getEntities;
        private readonly Func<string> _getString;
        private readonly string _idName;
        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();

        /// <param name="getUri"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            string idName)
            : this(getUri, checkEqual, getEntities,
                  () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"),
                  idName)
        {
        }

        /// <param name="getUri"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            Func<string> getString,
            string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            _checkEqual = checkEqual ?? throw new ArgumentNullException(nameof(checkEqual));
            _getEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _getString = getString ?? throw new ArgumentNullException(nameof(getString));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                       : idName;
        }

        public void ChangeGetDataFunc(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, bool> needDeleteJudger)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            var needDeleteItems = (from entity in Models
                                   where needDeleteJudger(entity)
                                   select entity).ToArray();
            foreach (var item in needDeleteItems)
            {
                Models.Remove(item);
            }
            page = 0;
        }

        public void Reset(int p = 1)
        {
            page = p;
            lastItem = firstItem = string.Empty;

            var temp = Models.Except(from m in Models
                                     where m.EntityFixed
                                     select m).ToArray();
            foreach (var item in temp)
            {
                Models.Remove(item);
            }
        }

        private string GetId(JToken token)
        {
            if (token == null) { return string.Empty; }
            else if ((token as JObject).TryGetValue(_idName, out JToken jToken))
            {
                return jToken.ToString();
            }
            else
            {
                throw new ArgumentException(nameof(_idName));
            }
        }

        public async Task Refresh(int p = -1)
        {
            if (p == -2) { Reset(0); }

            var array = (JArray)await DataHelper.GetDataAsync(_getUri(p, page, firstItem, lastItem), p == -2);

            if (p < 0) { page++; }

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
                                         where _checkEqual(m, b)
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
                    firstItem = GetId(array.First);
                    if (page == 1)
                    {
                        lastItem = GetId(array.Last);
                    }

                    int modelIndex = 0;

                    for (int i = 0; i < array.Count; i++)
                    {
                        var entities = _getEntities((JObject)array[i]);
                        if (entities == null) { continue; }

                        foreach (var item in entities)
                        {
                            if (item == null) { continue; }

                            Models.Insert(modelIndex + fixedNum, item);
                            modelIndex++;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(firstItem))
                    {
                        firstItem = GetId(array.First);
                    }
                    lastItem = GetId(array.Last);

                    foreach (JObject item in array)
                    {
                        var entities = _getEntities(item);
                        if (entities == null) { continue; }

                        foreach (var i in entities)
                        {
                            if (i == null) { continue; }
                            var b = fixedEntities.Any(k => k.EntityId == i.EntityId);
                            if (b) { continue; }

                            Models.Add(i);
                        }
                    }
                }
            }
            else if (p == -1)
            {
                page--;
                UIHelper.ShowMessage(_getString());
            }
        }
    }
}