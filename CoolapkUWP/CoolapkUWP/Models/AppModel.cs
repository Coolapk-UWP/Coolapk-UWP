using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    public class AppModel : Entity, IHasDescription
    {
        public string Url { get; private set; }
        public string Title { get; private set; }
        public string FollowNum { get; private set; }
        public string DownloadNum { get; private set; }
        public string VersionCode { get; private set; }
        public string VersionName { get; private set; }
        public string Description { get; private set; }
        public string LastUpdate { get; private set; }
        public ImageModel Logo { get; private set; }

        public ImageModel Pic => Logo;

        public AppModel(JObject token) : base(token)
        {
            if (token.TryGetValue("url", out JToken url))
            {
                Url = url.ToString();
            }

            if (token.TryGetValue("followCount", out JToken followCount))
            {
                FollowNum = followCount.ToString();
            }

            if (token.TryGetValue("downCount", out JToken downCount))
            {
                DownloadNum = downCount.ToString();
            }

            if (token.TryGetValue("apkversioncode", out JToken apkversioncode))
            {
                VersionCode = apkversioncode.ToString();
            }

            if (token.TryGetValue("apkversionname", out JToken apkversionname))
            {
                VersionName = apkversionname.ToString();
            }

            if (token.TryGetValue("title", out JToken title) && !string.IsNullOrEmpty(title.ToString()))
            {
                Title = title.ToString();
            }
            else if (token.TryGetValue("navTitle", out JToken navTitle) && !string.IsNullOrEmpty(navTitle.ToString()))
            {
                Title = navTitle.ToString();
            }

            if (token.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
            {
                Description = description.ToString();
            }
            else if (token.TryGetValue("keywords", out JToken keywords) && !string.IsNullOrEmpty(keywords.ToString()))
            {
                Description = keywords.ToString();
            }
            else if (token.TryGetValue("catName", out JToken catName) && !string.IsNullOrEmpty(catName.ToString()))
            {
                Description = catName.ToString();
            }
            else if (token.TryGetValue("apkTypeName", out JToken apkTypeName) && !string.IsNullOrEmpty(apkTypeName.ToString()))
            {
                Description = apkTypeName.ToString();
            }

            if (token.TryGetValue("logo", out JToken logo) && !string.IsNullOrEmpty(logo.ToString()))
            {
                Logo = new ImageModel(logo.ToString(), ImageType.Icon);
            }

            if (token.TryGetValue("lastupdate", out JToken lastupdate))
            {
                LastUpdate = lastupdate.ToObject<long>().ConvertUnixTimeStampToReadable();
            }
        }

        public override string ToString() => Title;
    }
}
