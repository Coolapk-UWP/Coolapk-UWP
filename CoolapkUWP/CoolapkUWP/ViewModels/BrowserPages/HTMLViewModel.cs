﻿using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public class HTMLViewModel : IViewModel
    {
        private Uri uri;

        private string title;
        public string Title
        {
            get => title;
            private set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string html;
        public string HTML
        {
            get => html;
            private set
            {
                if (html != value)
                {
                    html = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private string rawHTML;
        public string RawHTML
        {
            get => rawHTML;
            private set
            {
                if (rawHTML != value)
                {
                    rawHTML = value;
                    RaisePropertyChangedEvent();
                    _ = GetHtmlAsync(value, ThemeHelper.IsDarkTheme() ? "Dark" : "Light");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public HTMLViewModel(string url)
        {
            uri = url.ValidateAndGetUri();
            ThemeHelper.UISettingChanged.Add(mode =>
            {
                switch (mode)
                {
                    case UISettingChangedType.LightMode:
                    case UISettingChangedType.DarkMode:
                        _ = DispatcherHelper.ExecuteOnUIThreadAsync(async () => await GetHtmlAsync(RawHTML, ThemeHelper.IsDarkTheme() ? "Dark" : "Light"));
                        break;
                    case UISettingChangedType.NoPicChanged:
                        break;
                }
            });
        }

        public async Task Refresh(bool reset)
        {
            if (uri != null)
            {
                await Load_HTML(uri);
            }
        }

        private async Task Load_HTML(Uri uri)
        {
            UIHelper.ShowProgressBar();
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JObject json = JObject.Parse(result);
                if (json.TryGetValue("html", out JToken html) && !string.IsNullOrEmpty(html.ToString()))
                {
                    RawHTML = html.ToString();
                }
                else if (json.TryGetValue("description", out JToken description) && !string.IsNullOrEmpty(description.ToString()))
                {
                    RawHTML = description.ToString();
                }
                else
                {
                    RawHTML = "<h1>网络错误</h1>";
                }

                if (json.TryGetValue("title", out JToken title))
                {
                    Title = title.ToString();
                }
            }
            UIHelper.HideProgressBar();
        }

        public async Task GetHtmlAsync(string html, string theme)
        {
            StorageFile indexFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/WebView/HTMLView.html"));
            string index = await FileIO.ReadTextAsync(indexFile);
#if !FEATURE2
            index = index.Replace("ms-appx-web:///Assets/WebView", "https://coolapkuwp.app");
#endif
            HTML = index.Replace("{{RenderTheme}}", theme).Replace("{{HTMLBody}}", html);
        }
    }
}
