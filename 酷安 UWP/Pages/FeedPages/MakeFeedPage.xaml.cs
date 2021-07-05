using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    internal enum MakeFeedMode
    {
        Feed,
        Reply,
        ReplyReply
    }

    internal class EmojiData
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
        private string feedId;
        private Flyout flyout;
        private readonly List<EmojiData> emojis = new List<EmojiData>();
        public MakeFeedPage() => InitializeComponent();

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
            foreach (string item in EmojiHelper.emojis)
            {
                string u = $"ms-appx:///Assets/Emoji/{item}{(EmojiHelper.oldEmojis.Contains(item) && SettingHelper.GetBoolen("IsUseOldEmojiMode") ? "2" : string.Empty)}.png";
                emojis.Add(new EmojiData { uri = u, emoji = new BitmapImage(new Uri(u)), name = item[0] == '(' ? $"#{item})" : item });
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            InputBox.Document.GetText(TextGetOptions.UseObjectText, out string contentText);
            contentText = contentText.Replace("\r", "\r\n");
            if (string.IsNullOrWhiteSpace(contentText)) { return; }
            using (MultipartFormDataContent content = new MultipartFormDataContent(GetBoundary()))
            {
                switch (mode)
                {
                    case MakeFeedMode.Feed:
                        content.Add(new StringContent(contentText), "message");
                        content.Add(new StringContent("feed"), "type");
                        content.Add(new StringContent("0"), "is_html_article");
                        if (await UIHelper.Post("/feed/createFeed", content))
                        {
                            Frame.GoBack();
                            UIHelper.ShowMessage("发送成功");
                        }
                        break;
                    case MakeFeedMode.Reply:
                        content.Add(new StringContent(contentText), "message");
                        if (await UIHelper.Post($"/feed/reply?id={feedId}&type=feed", content))
                        {
                            UIHelper.ShowMessage("发送成功");
                            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
                            flyout.Hide();
                        }
                        break;
                    case MakeFeedMode.ReplyReply:
                        content.Add(new StringContent(contentText), "message");
                        if (await UIHelper.Post($"/feed/reply?id={feedId}&type=reply", content))
                        {
                            UIHelper.ShowMessage("发送成功");
                            InputBox.Document.SetText(TextSetOptions.None, string.Empty);
                            flyout.Hide();
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private string GetBoundary()
        {
            byte[] vs = new byte[16];
            new Random().NextBytes(vs);
            StringBuilder builder = new StringBuilder();
            foreach (byte item in vs)
            { _ = builder.Append(Convert.ToString(item, 16)); }
            _ = builder.Insert(8, "-");
            _ = builder.Insert(13, "-");
            _ = builder.Insert(18, "-");
            _ = builder.Insert(23, "-");
            return builder.ToString();
        }

        private void Flyout_Opened(object sender, object e)
        {
            GridView gridView = (sender as Flyout).Content as GridView;
            if (gridView.ItemsSource is null)
            { gridView.ItemsSource = emojis; }
        }

        private async void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {
            EmojiData c = e.ClickedItem as EmojiData;
            InputBox.Document.Selection.InsertImage(20, 20, 0, VerticalCharacterAlignment.Baseline, c.name, await (await StorageFile.GetFileFromApplicationUriAsync(new Uri(c.uri))).OpenReadAsync());
            _ = InputBox.Document.Selection.MoveRight(TextRangeUnit.Character, 1, false);
        }

        private void InputBox_ContextMenuOpening(object sender, ContextMenuEventArgs e) => e.Handled = true;
    }
}
