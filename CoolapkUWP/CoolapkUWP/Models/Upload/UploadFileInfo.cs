using Newtonsoft.Json;

namespace CoolapkUWP.Models.Upload
{
    public class UploadFileInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("resolution")]
        public string Resolution { get; set; }

        [JsonProperty("md5")]
        public string MD5 { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("uploadFileName")]
        public string UploadFileName { get; set; }
    }
}
