using Newtonsoft.Json.Linq;
using System;

namespace CoolapkUWP.Models.Users
{
    public class ContactModel : Entity
    {
        public int DateLine { get; private set; }
        public bool IsFriend { get; private set; }
        public UserModel UserInfo { get; private set; }

        public ContactModel(JObject token) : base(token)
        {
            if (token.TryGetValue("dateline", out JToken dateline))
            {
                DateLine = dateline.ToObject<int>();
            }

            if (token.TryGetValue("isfriend", out JToken isfriend))
            {
                IsFriend = Convert.ToBoolean(isfriend.ToObject<int>());
            }

            if (token.TryGetValue("userInfo", out JToken v1))
            {
                JObject userInfo = (JObject)v1;
                UserInfo = new UserModel(userInfo);
            }
        }

        public override string ToString() => UserInfo.ToString();
    }
}
