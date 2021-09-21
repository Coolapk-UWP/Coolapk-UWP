using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.Core.Providers
{
    public class CoolapkListProvider
    {
        private int page;
        private string firstItem, lastItem;
        private Func<int, int, string, string, Uri> _getUri;
        private readonly Func<Entity, JToken, bool> _checkEqual;
        private readonly Func<JObject, IEnumerable<Entity>> _getEntities;
        private readonly MessageType _messageType;
        private readonly string _idName;
        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();

        /// <param name="getUri"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            string idName)
            : this(getUri, checkEqual, getEntities, MessageType.NoMore, idName)
        {
        }

        /// <param name="getUri"> 获取Jarray的方法。参数顺序是 page, firstItem, lastItem。 </param>
        public CoolapkListProvider(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, JToken, bool> checkEqual,
            Func<JObject, IEnumerable<Entity>> getEntities,
            MessageType messageType,
            string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            _checkEqual = checkEqual ?? throw new ArgumentNullException(nameof(checkEqual));
            _getEntities = getEntities ?? throw new ArgumentNullException(nameof(getEntities));
            _messageType = messageType;
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                       : idName;
        }

        public void ChangeGetDataFunc(
            Func<int, int, string, string, Uri> getUri,
            Func<Entity, bool> needDeleteJudger)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            Entity[] needDeleteItems = (from entity in Models
                                        where needDeleteJudger(entity)
                                        select entity).ToArray();
            foreach (Entity item in needDeleteItems)
            {
                _ = Models.Remove(item);
            }
            page = 0;
        }

        public void Reset(int p = 1)
        {
            page = p;
            lastItem = firstItem = string.Empty;

            Entity[] temp = Models.Except(from m in Models
                                          where m.EntityFixed
                                          select m).ToArray();
            foreach (Entity item in temp)
            {
                _ = Models.Remove(item);
            }
        }

        public void Clear()
        {
            page = 0;
            lastItem = firstItem = string.Empty;
            Models.Clear();
        }

        private string GetId(JToken token)
        {
            return token == null
                ? string.Empty
                : (token as JObject).TryGetValue(_idName, out JToken jToken)
                    ? jToken.ToString()
                    : (token as JObject).TryGetValue("entityId", out JToken v1)
                                    ? v1.ToString()
                                    : (token as JObject).TryGetValue("id", out JToken v2) ? v2.ToString() : throw new ArgumentException(nameof(_idName));
        }

        public async Task Refresh(IEnumerable<(string, string)> cookies, int p = -1)
        {
            if (p == -2) { Reset(0); }

            (bool isSucceed, JToken result) = await Utils.GetDataAsync(_getUri(p, page, firstItem, lastItem), p == -2, cookies);
            if (!isSucceed) { return; }

            JArray array = (JArray)result;
            if (p < 0) { page++; }

            if (array != null && array.Count > 0)
            {
                Entity[] fixedEntities = (from m in Models
                                          where m.EntityFixed
                                          select m).ToArray();
                int fixedNum = fixedEntities.Length;
                foreach (Entity item in fixedEntities)
                {
                    Models.Remove(item);
                }

                Entity[] needDeleteEntites = (from m in Models
                                              from b in array
                                              where _checkEqual(m, b)
                                              select m).ToArray();
                foreach (Entity item in needDeleteEntites)
                {
                    _ = Models.Remove(item);
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
                        IEnumerable<Entity> entities = _getEntities((JObject)array[i]);
                        if (entities == null) { continue; }

                        foreach (Entity item in entities)
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
                        IEnumerable<Entity> entities = _getEntities(item);
                        if (entities == null) { continue; }

                        foreach (Entity i in entities)
                        {
                            if (i == null) { continue; }
                            bool b = fixedEntities.Any(k => k.EntityId == i.EntityId);
                            if (b) { continue; }

                            Models.Add(i);
                        }
                    }
                }
            }
            else if (p == -1)
            {
                page--;
                Utils.ShowInAppMessage(_messageType);
            }
        }
    }
}