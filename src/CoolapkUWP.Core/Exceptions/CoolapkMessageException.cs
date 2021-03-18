using Newtonsoft.Json.Linq;
using System;

namespace CoolapkUWP.Core.Exceptions
{
    public sealed class CoolapkMessageException : Exception
    {
        public const string RequestCaptcha = "err_request_captcha";

        public string MessageStatus { get; }

        public CoolapkMessageException(string message) : base(message) { }

        public CoolapkMessageException(string message, Exception innerException) : base(message, innerException) { }

        public CoolapkMessageException(JObject o) : base(o?.Value<string>("message") ?? string.Empty)
        {
            if (o != null && o.TryGetValue("messageStatus", out JToken token))
            {
                MessageStatus = token.ToString();
            }
        }

        public CoolapkMessageException(JObject o, Exception innerException) : base(o?.Value<string>("message") ?? string.Empty, innerException)
        {
            if (o != null && o.TryGetValue("messageStatus", out JToken token))
            {
                MessageStatus = token.ToString();
            }
        }
    }
}
