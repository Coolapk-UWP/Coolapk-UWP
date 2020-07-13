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

        public async Task Search(string keyWord)
        {
            var array = (JArray)await DataHelper.GetDataAsync(_getUri(keyWord, ++Page, lastItem), true);
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
                UIHelper.ShowMessage(Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"));
            }
        }
    }
}