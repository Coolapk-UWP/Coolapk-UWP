using CoolapkUWP.Control.ViewModels;
using CoolapkUWP.Data;
using System;
using System.ComponentModel;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingPages
{
    class Test
    {
        public string Value { get; set; }
    }
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        Test test = null;
        Test Test
        {
            get => test;
            set
            {
                test = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Test"));
            }
        }
        public TestPage()
        {
            this.InitializeComponent();
            url.Text = "/page/dataList?url=#/feed/digestList?type=0&is_html_article=0&tag=今日热点&message_status=all&title=有何高见&page=1";
            Tools.mainPage.ResetRowHeight();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Tools.rootPage.Navigate(typeof(FeedPages.UserPage), await Tools.GetUserIDByName(uid.Text));
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
                Frame.GoBack();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            string t = txb1.Text;
            UnicodeEncoding encoding = new UnicodeEncoding(true, false);
            while (t.Contains("\\u"))
            {
                int a = t.IndexOf("\\u");
                char b = t[a + 2];
                char c = t[a + 3];
                char d = t[a + 4];
                char f = t[a + 5];
                string g1 = new string(new char[] { b, c });
                string g2 = new string(new char[] { d, f });
                byte h1 = j(g1);
                byte h2 = j(g2);
                string i = encoding.GetString(new byte[] { h1, h2 }, 0, 2);
                t = t.Replace("\\u" + g1 + g2, i.ToString());
            }
            txb2.Text = t;

            byte j(string s)
            {
                byte r = 0;
                for (int i = 0; i < 2; i++)
                {
                    switch (s[i])
                    {
                        case '0':
                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                        case '6':
                        case '7':
                        case '8':
                        case '9':
                            r += (byte)(byte.Parse(s[i].ToString()) * (i == 0 ? 16 : 1));
                            break;
                        case 'a':
                            r += (byte)(10 * (i == 0 ? 16 : 1));
                            break;
                        case 'b':
                            r += (byte)(11 * (i == 0 ? 16 : 1));
                            break;
                        case 'c':
                            r += (byte)(12 * (i == 0 ? 16 : 1));
                            break;
                        case 'd':
                            r += (byte)(13 * (i == 0 ? 16 : 1));
                            break;
                        case 'e':
                            r += (byte)(14 * (i == 0 ? 16 : 1));
                            break;
                        case 'f':
                            r += (byte)(15 * (i == 0 ? 16 : 1));
                            break;
                    }
                }
                return r;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Tools.rootPage.ShowMessage(message.Text);
            GC.Collect();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Tools.rootPage.Navigate(typeof(FeedPages.DyhPage), "1324");
        }

        private async void Button_Click_5(object sender, RoutedEventArgs e)
        {
            txb1.Text = await Tools.GetJson(url.Text);
        }

        private void ScrollViewer_ViewChanging(object sender, ScrollViewerViewChangingEventArgs e)
            => Tools.mainPage.ChangeRowHeight(e.FinalView.VerticalOffset - (sender as ScrollViewer).VerticalOffset);

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            FeedReplyViewModel[] feed = new FeedReplyViewModel[] { new FeedReplyViewModel(Windows.Data.Json.JsonObject.Parse(txb2.Text)) };
            asa.ItemsSource = feed;
        }

        private void Slider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            Test = new Test { Value = e.NewValue.ToString() };
        }
    }
}