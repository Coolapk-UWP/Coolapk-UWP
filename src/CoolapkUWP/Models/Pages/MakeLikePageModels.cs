using CoolapkUWP.Core.Helpers;
using CoolapkUWP.Core.Models;
using CoolapkUWP.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkUWP.Models.Pages.MakeLikePageModels
{
    public class MakeLikePageModels : INotifyPropertyChanged
    {
        private string id;
        public string ID
        {
            get => id;
            set
            {
                id = value;
                RaisePropertyChangedEvent();
            }
        }

        private string username;
        public string Username
        {
            get => username;
            set
            {
                username = value;
                RaisePropertyChangedEvent();
            }
        }

        private int number;
        public int Number
        {
            get => number;
            set
            {
                number = value;
                RaisePropertyChangedEvent();
            }
        }

        public MakeLikePageModels(Uri uri)
        {
            if (!string.IsNullOrEmpty(uri.ToString())) { GetJson(uri); }
        }

        private async void GetJson(Uri uri)
        {
            bool isSucceed;
            string result;
            (isSucceed, result) = await DataHelper.GetHtmlAsync(uri, "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json);
            }
        }

        private void ReadJson(JObject token)
        {
            if (token.TryGetValue("data", out JToken data))
            {
                Number = 0;
                foreach (JObject v in (JArray)data)
                {
                    if (v.TryGetValue("entityType", out JToken entityType))
                    {
                        if (entityType.ToString() == "feed" || entityType.ToString() == "discovery")
                        {
                            Number++;
                            if (v.TryGetValue("id", out JToken id))
                            {
                                ID = id.ToString();
                                MakeLike(ID);
                            }
                            if (v.TryGetValue("userInfo", out JToken v2))
                            {
                                JObject userInfo = (JObject)v2;
                                if (userInfo.TryGetValue("username", out JToken username))
                                {
                                    Username = username.ToString();
                                }
                            }
                        }
                    }
                }
            }
        }

        private async void MakeLike(string id)
        {
            bool isSucceed;
            string result;
            (isSucceed, result) = await DataHelper.GetHtmlAsync(UriHelper.GetUri(UriType.OperateLike, string.Empty,id), "XMLHttpRequest");
            if (isSucceed && !string.IsNullOrEmpty(result))
            {
                JObject json = JObject.Parse(result);
                ReadJson(json);
            }
        }

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
