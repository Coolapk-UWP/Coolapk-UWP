using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages;
using CoolapkUWP.ViewModels.DataSource;
using CoolapkUWP.ViewModels.Providers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.FeedPages
{
    public class ProfileFlyoutViewModel : DataSourceBase<Entity>, IViewModel
    {
        private readonly CoolapkListProvider Provider;

        public string Title => throw new NotImplementedException();

        private bool isLogin;
        public bool IsLogin
        {
            get => isLogin;
            private set
            {
                if (isLogin != value)
                {
                    isLogin = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private ProfileDetailModel profileDetail;
        public ProfileDetailModel ProfileDetail
        {
            get => profileDetail;
            private set
            {
                if (profileDetail != value)
                {
                    profileDetail = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        internal ProfileFlyoutViewModel()
        {
            Provider = new CoolapkListProvider(
                (_, __, ___) => UriHelper.GetUri(UriType.GetMyPageCard),
                GetEntities,
                "entityType");
        }

        public async Task Refresh(bool reset)
        {
            IsLogin = await SettingsHelper.CheckLoginAsync();
            if (IsLogin)
            {
                ProfileDetail = await GetFeedDetailAsync(SettingsHelper.Get<string>(SettingsHelper.Uid));
                await Reset();
            }
            else
            {
                ProfileDetail = null;
                Clear();
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => Equals(other);

        private static async Task<ProfileDetailModel> GetFeedDetailAsync(string id)
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetUserProfile, id), true);
            if (!isSucceed) { return null; }

            JObject detail = (JObject)result;
            return detail != null ? new ProfileDetailModel(detail) : null;
        }

        private IEnumerable<Entity> GetEntities(JObject json)
        {
            yield return GetEntity(json);
        }

        private static Entity GetEntity(JObject json)
        {
            switch (json.Value<string>("entityType"))
            {
                case "entity_type_user_card_manager": return null;
                default:
                    if (json.TryGetValue("entityTemplate", out JToken entityTemplate))
                    {
                        switch (entityTemplate.Value<string>())
                        {
                            case "imageTextGridCard":
                            case "imageSquareScrollCard":
                            case "iconScrollCard":
                            case "iconGridCard":
                            case "feedScrollCard":
                            case "imageTextScrollCard":
                            case "iconMiniLinkGridCard":
                            case "iconMiniGridCard": return new IndexPageHasEntitiesModel(json, EntityType.Others);
                            case "iconListCard":
                            case "textLinkListCard": return new IndexPageHasEntitiesModel(json, EntityType.TextLinks);
                            case "titleCard": return new IndexPageOperationCardModel(json, OperationType.ShowTitle);
                            default: return null;
                        }
                    }
                    else { return null; }
            }
        }

        protected override async Task<IList<Entity>> LoadItemsAsync(uint count)
        {
            List<Entity> Models = new List<Entity>();
            if (_currentPage <= 1)
            {
                await Provider.GetEntity(Models, _currentPage++);
            }
            return Models;
        }

        protected override async Task AddItemsAsync(IList<Entity> items)
        {
            if (items != null)
            {
                foreach (Entity item in items)
                {
                    if (item is NullEntity) { continue; }
                    await AddAsync(item);
                }
            }
        }
    }
}
