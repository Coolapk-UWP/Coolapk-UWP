using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    enum MakeFeedMode
    {
        Feed,
        Reply,
        ReplyReply
    }
    class EmojiData
    {
        public string name;
        public string uri;
        public ImageSource emoji;
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MakeFeedPage : Page
    {
        MakeFeedMode mode;
        string feedId;
        Flyout flyout;
        List<EmojiData> emojis = new List<EmojiData>();
        public MakeFeedPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            mode = (MakeFeedMode)vs[0];
            if (vs.Length > 1)
            {
                feedId = vs[1] as string;
                flyout = vs[2] as Flyout;
            }
            else InputBox.Margin = Settings.stackPanelMargin;
            foreach (var item in Emojis.emojis)
            {
                string u = $"ms-appx:///Assets/Emoji/{item}{(Emojis.oldEmojis.Contains(item) && Settings.GetBoolen("IsUseOldEmojiMode") ? "2" : string.Empty)}.png";
                emojis.Add(new EmojiData { uri = u, emoji = new BitmapImage(new Uri(u)), name = item[0] == '(' ? $"#{item})" : item });
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) return;
            using (MultipartFormDataContent content = new MultipartFormDataContent(GetBoundary()))
            {
                switch (mode)
                {
                    case MakeFeedMode.Feed:
                        content.Add(new StringContent(contentText), "message");
                        content.Add(new StringContent("feed"), "type");
                        content.Add(new StringContent("0"), "is_html_article");
                        if (await Tools.Post("/feed/createFeed", content))
                        {
                            Frame.GoBack();
                            Tools.ShowMessage("发送成功");
                        }
                        break;
                    case MakeFeedMode.Reply:
                        content.Add(new StringContent(contentText), "message");
                        if (await Tools.Post($"/feed/reply?id={feedId}&type=feed", content))
                        {
                            Tools.ShowMessage("发送成功");
                            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
                            flyout.Hide();
                        }
                        break;
                    case MakeFeedMode.ReplyReply:
                        content.Add(new StringContent(contentText), "message");
                        if (await Tools.Post($"/feed/reply?id={feedId}&type=reply", content))
                        {
                            Tools.ShowMessage("发送成功");
                            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
                            flyout.Hide();
                        }
                        break;
                }
            }
        }

        string GetBoundary()
        {
            byte[] vs = new byte[16];
            new Random().NextBytes(vs);
            StringBuilder builder = new StringBuilder();
            foreach (var item in vs)
                builder.Append(Convert.ToString(item, 16));
            builder.Insert(8, "-");
            builder.Insert(13, "-");
            builder.Insert(18, "-");
            builder.Insert(23, "-");
            return builder.ToString();
        }

        private void Flyout_Opened(object sender, object e)
        {
            GridView gridView = ((sender as Flyout).Content as GridView);
            if (gridView.ItemsSource is null)
                gridView.ItemsSource = emojis;
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            EmojiData c = e.ClickedItem as EmojiData;
            InputBox.Document.Selection.InsertImage(20, 20, 0, VerticalCharacterAlignment.Baseline, c.name, await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(c.uri))).OpenReadAsync());
            InputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
        }

        private void InputBox_ContextMenuOpening(object sender, ContextMenuEventArgs e) => e.Handled = true;
    }
}
