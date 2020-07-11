using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Providers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.NotificationsPage
{
    internal enum ListType
    {
        Comment,
        AtMe,
        AtCommentMe,
        Like,
        Follow,
    }

    internal class ViewModel : IViewModel
    {
        private readonly CoolapkListProvider provider;

        public ObservableCollection<Entity> Models { get => provider?.Models ?? null; }
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; } = string.Empty;
        public ListType ListType { get; }

        internal ViewModel(ListType type)
        {
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage");
            switch (type)
            {
                case ListType.Comment:
                    Title = loader.GetString("notification");
                    provider =
                        new CoolapkListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetNotifications, 
                                    p == -2 ? true : false,
                                    "list",
                                    p < 0 ? ++page : p,
                                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => (a as NotificationModel).Id == b.Value<int>("id"),
                            (o) => new Entity[] { new SimpleNotificationModel(o) },
                            "id");
                    break;

                case ListType.AtMe:
                    Title = loader.GetString("AtMeText");
                    provider =
                        new CoolapkListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetNotifications,
                                    p == -2 ? true : false,
                                    "atMeList",
                                    p < 0 ? ++page : p,
                                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => (a as FeedModel).EntityId == $"{b.Value<int>("id")}",
                            (o) => new Entity[] { new FeedModel(o) },
                            "id");
                    break;

                case ListType.AtCommentMe:
                    Title = loader.GetString("AtMeCommentText");
                    provider =
                        new CoolapkListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetNotifications,
                                    p == -2 ? true : false,
                                    "atCommentMeList",
                                    p < 0 ? ++page : p,
                                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => (a as NotificationModel).Id == b.Value<int>("id"),
                            (o) => new Entity[] { new AtCommentMeNotificationModel(o) },
                            "id");

                    break;

                case ListType.Like:
                    Title = loader.GetString("LikedText");
                    provider =
                        new CoolapkListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetNotifications,
                                    p == -2 ? true : false,
                                    "feedLikeList",
                                    p < 0 ? ++page : p,
                                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => (a as NotificationModel).Id == b.Value<int>("id"),
                            (o) => new Entity[] { new LikeNotificationModel(o) },
                            "id");

                    break;

                case ListType.Follow:
                    Title = loader.GetString("FollowedText");
                    provider =
                        new CoolapkListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetNotifications,
                                    p == -2 ? true : false,
                                    "contactsFollowList",
                                    p < 0 ? ++page : p,
                                    string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                                    string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => (a as NotificationModel).Id == b.Value<int>("id"),
                            (o) => new Entity[] { new SimpleNotificationModel(o) },
                            "id");
                    break;

                default:throw new ArgumentException(nameof(type));
            }
            ListType = type;
        }

        public async Task Refresh(int p = -1) => await provider?.Refresh(p);

    }
}
