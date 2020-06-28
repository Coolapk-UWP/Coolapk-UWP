using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.MyPage
{
    public class ViewMode : IViewModel
    {
        public double[] VerticalOffsets { get; set; } = new double[1];

        public string Title => string.Empty;

        internal Models.Controls.UserHubModel UserModel { get; private set; }

        internal Helpers.Providers.CoolapkListProvider provider;

        private static Entity GetEntity(JObject jo)
        {
            switch (jo.Value<string>("entityType"))
            {
                case "entity_type_user_card_manager": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                default:
                    if (jo.TryGetValue("entityTemplate", out JToken v1))
                    {
                        switch (v1.Value<string>())
                        {
                            case "imageTextGridCard":
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(jo, EntityType.Others);
                            case "iconListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(jo, EntityType.TextLinks);
                            case "titleCard": return new IndexPageOperationCardModel(jo, OperationType.ShowTitle);
                            default: return null;
                        }
                    }
                    else return null;
            }
        }

        internal ViewMode()
        {
            provider =
                new Helpers.Providers.CoolapkListProvider(
                    async (_, __, ___, ____) => (JArray)await DataHelper.GetDataAsync(DataUriType.GetMyPageCard, true),
                    (_, __) => false,
                    (o) => new Entity[] { GetEntity(o) },
                    "entityType");
        }

        public async Task Refresh(int p = -1)
        {
            var o = (JObject)await DataHelper.GetDataAsync(DataUriType.GetUserProfile, true, SettingsHelper.Get<string>(SettingsHelper.Uid));
            string url = o.Value<string>("userAvatar");
            var bitmapImage = await ImageCacheHelper.GetImageAsync(ImageType.BigAvatar, url);
            UserModel = new Models.Controls.UserHubModel(o, bitmapImage);

            provider.Reset();
            await provider.Refresh();
        }
    }
}
