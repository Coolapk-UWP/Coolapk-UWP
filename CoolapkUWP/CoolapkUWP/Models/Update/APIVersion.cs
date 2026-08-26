using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CoolapkUWP.Models.Update
{
    public class APIVersion
    {
        public string Version { get; set; }
        public int VersionCode { get; set; }

        public string MajorVersion
        {
            get
            {
                string result = Version.Split('.').FirstOrDefault();
                return result == "1" ? "9" : result;
            }
        }

        public APIVersion() { }

        public APIVersion(string version, int versionCode) : this()
        {
            Version = version;
            VersionCode = versionCode;
        }

        public static APIVersion Parse(string line)
        {
            Match match = Regex.Match(line, @"\+CoolMarket/(.*)-universal");
            if (match.Success && match.Groups.Count >= 2)
            {
                string value = match.Groups[1].Value;
                string[] lines = value.Split('-');
                if (lines.Length >= 2)
                {
                    return new APIVersion(lines[0].Trim(), int.Parse(lines[1].Trim()));
                }
            }
            return null;
        }

        public static APIVersion Create(APIVersions version)
        {
            switch (version)
            {
                case APIVersions.Custom:
                    return SettingsHelper.Get<APIVersion>(SettingsHelper.CustomAPI);
                case APIVersions.小程序:
                    return new APIVersion("1.0", 1902250);
                case APIVersions.V6:
                    return new APIVersion("6.10.6", 1608291);
                case APIVersions.V7:
                    return new APIVersion("7.9.6_S", 1710201);
                case APIVersions.V8:
                    return new APIVersion("8.7", 1809041);
                case APIVersions.V9:
                    return new APIVersion("9.6.3", 1910291);
                case APIVersions.V10:
                    return new APIVersion("10.5.3", 2009271);
                case APIVersions.V11:
                    return new APIVersion("11.4.7", 2112231);
                case APIVersions.V12:
                    return new APIVersion("12.5.4", 2212261);
                case APIVersions.V13:
                    return new APIVersion("13.4.1", 2312121);
                case APIVersions.V14:
                    return new APIVersion("14.6.0", 2411221);
                case APIVersions.V15:
                    return new APIVersion("15.9.5", 2601051);
                case APIVersions.V16:
                    return new APIVersion("16.5.1", 2607271);
                default:
                    goto case APIVersions.Custom;
            }
        }

        public static async Task<APIVersion> GetLatestAsync()
        {
            (bool isSucceed, JToken result) = await RequestHelper.GetDataAsync(UriHelper.GetUri(UriType.GetAppDetail, "com.coolapk.market"));
            if (isSucceed)
            {
                AppModel model = new AppModel((JObject)result);
                if (!string.IsNullOrEmpty(model.VersionCode) && int.TryParse(model.VersionCode, out int versionCode) && !string.IsNullOrEmpty(model.VersionName))
                {
                    return new APIVersion(model.VersionName, versionCode);
                }
            }
            return null;
        }

        public override string ToString() => $"+CoolMarket/{Version}-{VersionCode}-universal";
    }

    public enum APIVersions
    {
        Custom = 4,
        小程序,
        V6 = 6,
        V7,
        V8,
        V9,
        V10,
        V11,
        V12,
        V13,
        V14,
        V15,
        V16
    }
}
