using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class AppModel : Entity
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string Introduce { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public AppModel(JObject o) : base(o)
        {
            if (o.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }
            if (o.TryGetValue("followCount", out JToken followCount) && !string.IsNullOrEmpty(followCount.ToString()))
            {
                FollowNum = followCount.ToString();
            }
            if (o.TryGetValue("downCount", out JToken downCount) && !string.IsNullOrEmpty(downCount.ToString()))
            {
                DownloadNum = downCount.ToString();
            }
            if (o.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            else if (o.TryGetValue("navTitle", out JToken navTitle) && !string.IsNullOrEmpty(navTitle.ToString()))
            {
                Title = navTitle.ToString();
            }
            if (o.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Introduce = description.ToString();
            }
            else if (o.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Introduce = keywords.ToString();
            }
            else if (o.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Introduce = catName.ToString();
            }
            else if (o.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Introduce = apkTypeName.ToString();
            }
            if (o.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }
            if (o.TryGetValue("lastupdate", out JToken lastupdate) && !string.IsNullOrEmpty(lastupdate.ToString()))
            {
                LastUpdate = DataHelper.ConvertUnixTimeStampToReadable(o.Value<int>("lastupdate"));
            }
        }
    }
}