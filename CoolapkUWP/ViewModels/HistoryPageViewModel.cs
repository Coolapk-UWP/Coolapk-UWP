using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.HistoryPage
{
    internal class ViewModel : IViewModel
    {
        private CoolapkListProvider provider;
        public ObservableCollection<Entity> Models { get => provider?.Models ?? null; }

        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; }

        internal ViewModel(string title)
        {
            if (string.IsNullOrEmpty(title)) { throw new ArgumentException(nameof(title)); }

            Title = title;

            DataUriType type = DataUriType.CheckLoginInfo;

            switch (title)
            {
                case "我的常去":
                    type = DataUriType.GetUserRecentHistory;
                    break;
                case "浏览历史":
                    type = DataUriType.GetUserHistory;
                    break;
                default: throw new ArgumentException(nameof(title));
            }

            provider =
                new CoolapkListProvider(
                    async (p, page, firstItem, lastItem) =>
                    (JArray)await DataHelper.GetDataAsync(
                        type,
                        p == -2 ? true : false,
                        p < 0 ? ++page : p,
                        string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                        string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    (a, b) => ((HistoryModel)a).Id == b.Value<string>("id"),
                    (o) => new Entity[] { new HistoryModel(o) },
                    "id");
        }

        public async Task Refresh(int p = -1) => await provider?.Refresh(p);
    }
}