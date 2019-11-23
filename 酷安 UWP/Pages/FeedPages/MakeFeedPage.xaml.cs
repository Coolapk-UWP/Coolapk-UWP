using CoolapkUWP.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MakeFeedPage : Page
    {
        MakeFeedMode mode;
        string feedId;
        Flyout flyout;
        public MakeFeedPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            object[] vs = e.Parameter as object[];
            mode = (MakeFeedMode)vs[0];
            switch (mode)
            {
                case MakeFeedMode.Feed:
                    InputBox.Padding = new Thickness(0, Settings.PageTitleHeight, 0, 48);
                    break;
                default:
                    feedId = vs[1] as string;
                    flyout = vs[2] as Flyout;
                    break;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputBox.Text)) return;
            string contentText = InputBox.Text.Replace("\r", "\r\n");
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
                            InputBox.Text = string.Empty;
                            flyout.Hide();
                        }
                        break;
                    case MakeFeedMode.ReplyReply:
                        content.Add(new StringContent(contentText), "message");
                        if (await Tools.Post($"/feed/reply?id={feedId}&type=reply", content))
                        {
                            Tools.ShowMessage("发送成功");
                            InputBox.Text = string.Empty;
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
    }
}
