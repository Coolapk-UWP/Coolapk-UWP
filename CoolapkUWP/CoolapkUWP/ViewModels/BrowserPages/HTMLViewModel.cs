using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace CoolapkUWP.ViewModels.BrowserPages
{
    public sealed class HTMLViewModel : IViewModel
    {
        public CoreDispatcher Dispatcher { get; }

        private readonly Uri uri;

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

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public HTMLViewModel(string url, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            uri = url.ValidateAndGetUri();
            ThemeHelper.UISettingChanged += OnUISettingChanged;
        }

        ~HTMLViewModel()
        {
            ThemeHelper.UISettingChanged -= OnUISettingChanged;
        }

        private void OnUISettingChanged(ApplicationTheme mode)
        {
            switch (mode)
            {
                case ApplicationTheme.Light:
                    _ = GetHtmlAsync(RawHTML, "Light");
                    break;
                case ApplicationTheme.Dark:
                    _ = GetHtmlAsync(RawHTML, "Dark");
                    break;
            }
        }

        public async Task Refresh(bool reset)
        {
            if (uri != null)
            {
                await Load_HTML(uri);
            }
        }

        bool IViewModel.IsEqual(IViewModel other) => other is HTMLViewModel model && IsEqual(model);

        public bool IsEqual(HTMLViewModel other) => uri == other.uri;

        private async Task Load_HTML(Uri uri)
        {
            UIHelper.ShowProgressBar();
            (bool isSucceed, string result) = await RequestHelper.GetStringAsync(uri, "XMLHttpRequest");
            if (isSucceed)
            {
                JObject json = JObject.Parse(result);

                if (json.TryGetValue("title", out JToken title))
                {
                    Title = title.ToString();
                }

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
                    (isSucceed, result) = await RequestHelper.GetStringAsync(uri).ConfigureAwait(false);
                    if (isSucceed && !string.IsNullOrWhiteSpace(result))
                    {
                        HtmlDocument doc = new HtmlDocument();
                        doc.LoadHtml(result);
                        string content = doc.DocumentNode.ChildNodes.FindFirst("html")?.ChildNodes.FindFirst("body")?.InnerHtml;
                        if (!string.IsNullOrEmpty(content))
                        {
                            RawHTML = content;
                        }
                    }
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
