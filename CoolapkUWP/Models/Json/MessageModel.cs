using Newtonsoft.Json;

namespace CoolapkUWP.Models.Json.MessageModel
{
    [JsonObject(MemberSerialization.OptIn)]
    internal class Rootobject
    {
        [JsonProperty("status")]
        public int Status { get; set; }
        [JsonProperty("error")]
        public object Error { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("forwardUrl")]
        public string ForwardUrl { get; set; }
    }
}