using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CoolapkUWP.Models.Update
{
    public class UpdateInfo
    {
        [JsonProperty("url")]
        public string ApiUrl { get; set; }
        [JsonProperty("html_url")]
        public string ReleaseUrl { get; set; }
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
        [JsonProperty("prerelease")]
        public bool IsPreRelease { get; set; }
        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("published_at")]
        public DateTime PublishedAt { get; set; }
        [JsonProperty("assets")]
        public List<Asset> Assets { get; set; }
        [JsonProperty("body")]
        public string Changelog { get; set; }
        public bool IsExistNewVersion { get; set; }
    }

    public class Asset
    {
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("browser_download_url")]
        public string Url { get; set; }
    }
}
