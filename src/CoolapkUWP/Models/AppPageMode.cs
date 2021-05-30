using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class AppPageMode : Entity
    {
        public string DownloadUrl { get; private set; }
        public string Url { get; private set; }
        public string Title { get; private set; }
        public int EntityID { get; private set; }
        public string Version { get; private set; }
        public string Apksize { get; private set; }
        public string FollowNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string ChangeLog { get; private set; }
        public string Introduce { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public AppPageMode(JObject o) : base(o)
        {
            if (o.TryGetValue("url", out JToken url) && !string.IsNullOrEmpty(url.ToString()))
            {
                Url = url.ToString();
            }
            if (o.TryGetValue("id", out JToken id) && !string.IsNullOrEmpty(id.ToString()))
            {
                EntityID = o.Value<int>("id");
            }
            if (o.TryGetValue("followCount", out JToken followCount) && !string.IsNullOrEmpty(followCount.ToString()))
            {
                FollowNum = followCount.ToString();
            }
            if (o.TryGetValue("downCount", out JToken downCount) && !string.IsNullOrEmpty(downCount.ToString()))
            {
                DownloadNum = downCount.ToString();
            }
            if (o.TryGetValue("apkDetailDownloadUrl", out JToken apkDetailDownloadUrl) && !string.IsNullOrEmpty(apkDetailDownloadUrl.ToString()))
            {
                DownloadUrl = apkDetailDownloadUrl.ToString();
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
            if (o.TryGetValue("dataRow", out JToken v1))
            {
                JObject dataRow = (JObject)v1;
                if (dataRow.TryGetValue("id", out id) && !string.IsNullOrEmpty(id.ToString()))
                {
                    EntityID = (int)id;
                }
                if (dataRow.TryGetValue("version", out JToken version) && !string.IsNullOrEmpty(version.ToString()))
                {
                    Version = version.ToString();
                }
                if (dataRow.TryGetValue("apksize", out JToken apksize) && !string.IsNullOrEmpty(apksize.ToString()))
                {
                    Apksize = apksize.ToString();
                }
                if (dataRow.TryGetValue("changelog", out JToken changelog) && !string.IsNullOrEmpty(changelog.ToString()))
                {
                    ChangeLog = changelog.ToString();
                }
                if (dataRow.TryGetValue("introduce", out JToken introduce) && !string.IsNullOrEmpty(introduce.ToString()))
                {
                    Introduce = introduce.ToString();
                }
            }
        }
    }
}