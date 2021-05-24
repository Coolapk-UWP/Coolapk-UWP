using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;

namespace CoolapkUWP.Models
{
    internal class AppPageMode : Entity
    {
        public string DownloadUrl{ get; private set; }
        public string NavTitle { get; private set; }
        public int EntityID { get; private set; }
        public string Version { get; private set; }
        public string Apksize { get; private set; }
        public string ChangeLog { get; private set; }
        public string Introduce { get; private set; }
        public ImageModel Logo { get; private set; }

        public AppPageMode(JObject o) : base(o)
        {
            DownloadUrl = o.Value<string>("apkDetailDownloadUrl");
            NavTitle = o.Value<string>("navTitle");
            EntityID = o["dataRow"].Value<int>("id");
            Version = o["dataRow"].Value<string>("version");
            Apksize = o["dataRow"].Value<string>("apksize");
            ChangeLog = o["dataRow"].Value<string>("changelog");
            Introduce = o["dataRow"].Value<string>("introduce");
            Logo = new ImageModel(o.Value<string>("logo"), ImageType.BigAvatar);
        }
    }
}