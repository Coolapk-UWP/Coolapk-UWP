using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Core.Providers;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.Models.Pages.NotificationsPageModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
            string listName = string.Empty;
            Func<Entity, JToken, bool> checkEqual = null;
            Func<JObject, IEnumerable<Entity>> getEntities = null;

            switch (type)
            {
                case ListType.Comment:
                    Title = loader.GetString("notification");
                    listName = "list";
                    checkEqual = (a, b) => (a as NotificationModel).Id == b.Value<int>("id");
                    getEntities = (o) => new Entity[] { new SimpleNotificationModel(o) };
                    break;

                case ListType.AtMe:
                    Title = loader.GetString("AtMeText");
                    listName = "atMeList";
                    checkEqual = (a, b) => (a as FeedModel).EntityId == $"{b.Value<int>("id")}";
                    getEntities = (o) => new Entity[] { new FeedModel(o) };
                    break;

                case ListType.AtCommentMe:
                    Title = loader.GetString("AtMeCommentText");
                    listName = "atCommentMeList";
                    checkEqual = (a, b) => (a as NotificationModel).Id == b.Value<int>("id");
                    getEntities = (o) => new Entity[] { new AtCommentMeNotificationModel(o) };
                    break;

                case ListType.Like:
                    Title = loader.GetString("LikedText");
                    listName = "feedLikeList";
                    checkEqual = (a, b) => (a as NotificationModel).Id == b.Value<int>("id");
                    getEntities = (o) => new Entity[] { new LikeNotificationModel(o) };
                    break;

                case ListType.Follow:
                    Title = loader.GetString("FollowedText");
                    listName = "contactsFollowList";
                    checkEqual = (a, b) => (a as NotificationModel).Id == b.Value<int>("id");
                    getEntities = (o) => new Entity[] { new SimpleNotificationModel(o) };
                    break;

                default: throw new ArgumentException(nameof(type));
            }

            provider =
                new CoolapkListProvider(
                    (p, page, firstItem, lastItem) =>
                        UriHelper.GetUri(
                            UriType.GetNotifications,
                            listName,
                            p < 0 ? ++page : p,
                            string.IsNullOrEmpty(firstItem) ? string.Empty : $"&firstItem={firstItem}",
                            string.IsNullOrEmpty(lastItem) ? string.Empty : $"&lastItem={lastItem}"),
                    checkEqual, getEntities, "id");
            ListType = type;
        }

        public Task Refresh(int p = -1) => provider?.Refresh(p);
    }
}