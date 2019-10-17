using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using 酷安_UWP.UsersPage;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        MainPage mainPage;

        public TestPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { await CoolApkSDK.GetUserIDByName(uid.Text), mainPage });
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) => Frame.GoBack();

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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            foreach (var item in IndexPage.FeedsCollection)
            {
                txb2.Text += item.jObject.ToString();
                txb2.Text += "\n";
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            mainPage.Frame.Navigate(typeof(FeedDetailPage), new object[] { id.Text, mainPage, string.Empty });
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            JObject jObject = JObject.Parse(txb2.Text);
            IndexPage.FeedsCollection.Add(new Feed(jObject));
        }

        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            JObject jObject = JObject.Parse(txb2.Text);
            mainPage.Frame.Navigate(typeof(UserPage), new object[] { string.Empty, mainPage, new Feed(jObject) });
        }
    }
}
