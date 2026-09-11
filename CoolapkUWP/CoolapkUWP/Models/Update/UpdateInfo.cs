using Newtonsoft.Json;
using System;

namespace CoolapkUWP.Models.Update
{
#if CANARY
    public sealed class RunsInfo
    {
        [JsonProperty("count")]
        public long Count { get; set; }
        [JsonProperty("value")]
        public RunInfo[] Value { get; set; }
    }

    public sealed class RunInfo
    {
        [JsonProperty("_links")]
        public Links Links { get; set; }
        [JsonProperty("pipeline")]
        public Pipeline Pipeline { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("createdDate")]
        public DateTimeOffset CreatedDate { get; set; }
        [JsonProperty("finishedDate")]
        public DateTimeOffset FinishedDate { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("id")]
        public long ID { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public sealed class Links
    {
        [JsonProperty("self")]
        public Link Self { get; set; }
        [JsonProperty("web")]
        public Link Web { get; set; }
        [JsonProperty("pipeline.web")]
        public Link PipelineWeb { get; set; }
        [JsonProperty("pipeline")]
        public Link Pipeline { get; set; }
    }

    public sealed class Link
    {
        [JsonProperty("href")]
        public string Href { get; set; }
    }

    public sealed class Pipeline
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("id")]
        public long ID { get; set; }
        [JsonProperty("revision")]
        public long Revision { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("folder")]
        public string Folder { get; set; }
    }

    public sealed class Asset
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("signedContent")]
        public SignedContent SignedContent { get; set; }
        [JsonIgnore]
        public string DownloadUrl => SignedContent?.Url;
    }

    public sealed class SignedContent
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("signatureExpires")]
        public DateTimeOffset SignatureExpires { get; set; }
    }

    public sealed class UpdateInfo
    {
        public string ReleaseUrl { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset PublishedAt { get; set; }
        public Asset[] Assets { get; set; }
        public bool IsExistNewVersion { get; set; }
        public SystemVersionInfo Version { get; set; }
    }
#else
    public sealed class UpdateInfo
    {
        [JsonProperty("url")]
        public string ApiUrl { get; set; }
        [JsonProperty("html_url")]
        public string ReleaseUrl { get; set; }
        [JsonProperty("tag_name")]
        public string TagName { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("prerelease")]
        public bool IsPreRelease { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("published_at")]
        public DateTimeOffset PublishedAt { get; set; }
        [JsonProperty("assets")]
        public Asset[] Assets { get; set; }
        [JsonProperty("tarball_url")]
        public string TarUrl { get; set; }
        [JsonProperty("zipball_url")]
        public string ZipUrl { get; set; }
        [JsonProperty("body")]
        public string Changelog { get; set; }
        [JsonProperty("discussion_url")]
        public string DiscussionUrl { get; set; }
        [JsonIgnore]
        public bool IsExistNewVersion { get; set; }
        [JsonIgnore]
        public SystemVersionInfo Version { get; set; }
    }

    public sealed class Asset
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("size")]
        public long Size { get; set; }
        [JsonProperty("download_count")]
        public long DownloadCount { get; set; }
        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; }
        [JsonProperty("browser_download_url")]
        public string DownloadUrl { get; set; }
    }
#endif
}
