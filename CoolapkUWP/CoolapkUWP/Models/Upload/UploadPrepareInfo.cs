using Newtonsoft.Json;

namespace CoolapkUWP.Models.Upload
{
    public class UploadPrepareInfo
    {
        [JsonProperty("accessKeySecret")]
        public string AccessKeySecret { get; set; }

        [JsonProperty("accessKeyId")]
        public string AccessKeyID { get; set; }

        [JsonProperty("securityToken")]
        public string SecurityToken { get; set; }

        [JsonProperty("expiration")]
        public string Expiration { get; set; }

        [JsonProperty("uploadImagePrefix")]
        public string UploadImagePrefix { get; set; }

        [JsonProperty("endPoint")]
        public string EndPoint { get; set; }

        [JsonProperty("bucket")]
        public string Bucket { get; set; }

        [JsonProperty("callbackUrl")]
        public string CallbackUrl { get; set; }
    }
}
