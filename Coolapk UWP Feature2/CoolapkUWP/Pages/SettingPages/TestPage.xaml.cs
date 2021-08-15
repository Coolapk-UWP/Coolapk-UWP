using CoolapkUWP.Control;
using CoolapkUWP.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            InitializeComponent();
            tile.Text = ApplicationData.Current.LocalSettings.Values["TileUrl"].ToString();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            { Frame.GoBack(); }
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Uri uri = ValidateAndGetUri(url.Text);
            string s = uri == null ? "这不是一个链接" : await UIHelper.GetHTML(uri.ToString(), "XMLHttpRequest");
            if (string.IsNullOrEmpty(s))
            {
                s = "网络错误";
            }
            ContentDialog GetJsonDialog = new ContentDialog
            {
                Title = url.Text,
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = ConvertJsonString(s),
                        IsTextSelectionEnabled = true
                    },
                    VerticalScrollMode = ScrollMode.Enabled,
                    HorizontalScrollMode = ScrollMode.Enabled,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                },
                CloseButtonText = "好的",
                DefaultButton = ContentDialogButton.Close
            };
            _ = await GetJsonDialog.ShowAsync();
        }

        private static Uri ValidateAndGetUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString);
            }
            catch (FormatException)
            {
            }
            return uri;
        }

        private string ConvertJsonString(string str)
        {
            //格式化json字符串
            JsonSerializer serializer = new JsonSerializer();
            TextReader tr = new StringReader(str);
            JsonTextReader jtr = new JsonTextReader(tr);
            object obj = null;
            try { obj = serializer.Deserialize(jtr); } catch { }
            if (obj != null)
            {
                StringWriter textWriter = new StringWriter();
                JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
                {
                    Formatting = Formatting.Indented,
                    Indentation = 4,
                    IndentChar = ' '
                };
                serializer.Serialize(jsonWriter, obj);
                return textWriter.ToString();
            }
            else
            {
                return str;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["TileUrl"] = tile.Text;
            LiveTileControl.UpdateTile();
        }

        private void EmojisTest()
        {
            List<string> b;
            string emojis = string.Empty;
            b = EmojiHelper.normal.ToList();
            foreach (string item in EmojiHelper.coolcoins)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.funny)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.doge)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.tradition)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.classic)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in EmojiHelper.emojis)
            {
                if (!b.Contains(item))
                { b.Add(item); }
            }
            foreach (string item in b)
            {
                emojis += $"\"{item}\",\n";
            }
            ContentDialog GetJsonDialog = new ContentDialog
            {
                Title = $"{b.ToArray().Length}/{EmojiHelper.emojis.Length}",
                Content = new ScrollViewer
                {
                    Content = new TextBlock
                    {
                        Text = emojis,
                        IsTextSelectionEnabled = true
                    }
                },
                CloseButtonText = "好的",
                DefaultButton = ContentDialogButton.Close
            };
            _ = GetJsonDialog.ShowAsync();
        }

        private void uid_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click(sender, e);
            }
        }

        private void url_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_5(sender, e);
            }
        }

        private void message_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_4(sender, e);
            }
        }

        private void image_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_6(sender, e);
            }
        }

        private void tile_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                Button_Click_2(sender, e);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(FeedPages.FeedListPage), new object[] { FeedPages.FeedListType.UserPageList, await UIHelper.GetUserIDByName(uid.Text) });

        private void Button_Click_1(object sender, RoutedEventArgs e) => UIHelper.Navigate(typeof(AppPages.AppRecommendPage));

        private void Button_Click_4(object sender, RoutedEventArgs e) => UIHelper.ShowMessage(message.Text);

        private void Button_Click_6(object sender, RoutedEventArgs e) => UIHelper.ShowImage(image.Text, ImageType.OriginImage);
    }
}