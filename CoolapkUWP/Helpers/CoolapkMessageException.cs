using Newtonsoft.Json.Linq;
using System;

namespace CoolapkUWP.Helpers
{
    public class CoolapkMessageException : Exception
    {
        public CoolapkMessageException(string message) : base(message) { }

        public CoolapkMessageException(string message, Exception innerException) : base(message, innerException) { }

        public CoolapkMessageException(JToken token) : base(token?.ToString() ?? string.Empty) { }

        public CoolapkMessageException(JToken token, Exception innerException) : base(token?.ToString() ?? string.Empty, innerException) { }
    }
}
