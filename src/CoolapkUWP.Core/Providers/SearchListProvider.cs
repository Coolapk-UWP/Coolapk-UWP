using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.Core.Providers
{
    public class SearchListProvider
    {
        private string lastItem;
        public ObservableCollection<Entity> Models { get; } = new ObservableCollection<Entity>();
        public int Page { get; private set; }

        private readonly Func<string, int, string, Uri> _getUri;
        private readonly Func<JObject, Entity> _getEntity;
        private readonly string _idName;

        /// <param name="getUri"> keyword, page, lastItem </param>
        public SearchListProvider(
            Func<string, int, string, Uri> getUri,
            Func<JObject, Entity> getEntity,
            string idName)
        {
            _getUri = getUri ?? throw new ArgumentNullException(nameof(getUri));
            _getEntity = getEntity ?? throw new ArgumentNullException(nameof(getEntity));
            _idName = string.IsNullOrEmpty(idName) ? throw new ArgumentException($"{nameof(idName)}不能为空")
                                                   : idName;
        }

        public void Reset()
        {
            Page = 0;
            lastItem = string.Empty;
            Models.Clear();
        }

        public async Task Search(string keyWord, IEnumerable<(string, string)> cookies)
        {
            (bool isSucceed, JToken result) = await Utils.GetDataAsync(_getUri(keyWord, ++Page, lastItem), true, cookies);
            if (!isSucceed) { return; }

            JArray array = (JArray)result;
            if (Page == 1)
            {
                Models.Clear();
            }
            if (array.Count > 0)
            {
                lastItem = array.Last.Value<string>(_idName);
                foreach (JObject item in array)
                {
                    Models.Add(_getEntity(item));
                }
            }
            else
            {
                Page--;
                Utils.ShowInAppMessage(MessageType.NoMore);
            }
        }
    }
}