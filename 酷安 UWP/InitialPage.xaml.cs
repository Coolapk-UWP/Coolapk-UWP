using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InitialPage : Page
    {
        public InitialPage()
        {
            this.InitializeComponent();
        }
        MainPage mainPage;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            mainPage = e.Parameter as MainPage;
        }
        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = MainPivot;
            if (pivot.Items.Count == 1 && (pivot.Items[0] as PivotItem).Tag as string == "-1")
            {
                pivot.Items.Clear();
                string r = await CoolApkSDK.GetCoolApkMessage("/main/init");
                JArray array = JObject.Parse(r)["data"] as JArray;
                foreach (var item in array)
                {
                    if (item["entityTemplate"].ToString() == "configCard")
                    {
                        JArray jArray = item["entities"] as JArray;
                        foreach (var it in jArray)
                        {
                            switch (it["title"].ToString())
                            {
                                case "酷品":
                                case "看看号":
                                case "直播": continue;
                                default:
                                    break;
                            }
                            PivotItem i = new PivotItem
                            {
                                Header = it["title"].ToString(),
                                Tag = it["title"].ToString() == "头条" ? string.Empty : it["url"].ToString() + "&title=" + it["title"].ToString(),
                                Content = new Frame()
                            };
                            if (it["title"].ToString() == "关注")
                            {
                                Button button = new Button { Content = new TextBlock { Text = "▼" }, Background = null, Name = "moreB" };
                                MenuFlyout flyout = new MenuFlyout();
                                foreach (var ite in it["entities"])
                                    if (ite["entityType"].ToString() == "page")
                                    {
                                        MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem { Text = ite["title"].ToString(), Tag = ite["url"].ToString() + "&title=" + ite["title"].ToString() };
                                        menuFlyoutItem.Tapped += (s, _) =>
                                        {
                                            MenuFlyoutItem fI = s as MenuFlyoutItem;
                                            TextBlock textBlock = FindName("TitleTextBlock") as TextBlock;
                                            textBlock.Text = fI.Text;
                                            foreach (MenuFlyoutItem j in ((FindName("moreB") as Button).Flyout as MenuFlyout).Items)
                                                j.Icon = null;
                                            fI.Icon = new SymbolIcon(Symbol.Accept);
                                            string pageUrl = fI.Tag as string;
                                            if (pageUrl.IndexOf("/page") == -1)
                                                pageUrl = "/page/dataList?url=" + pageUrl;
                                            else if (pageUrl.IndexOf("/page") == 0 && !pageUrl.Contains("/page/dataList"))
                                                pageUrl = pageUrl.Replace("/page", "/page/dataList");
                                            pageUrl = pageUrl.Replace("#", "%23");
                                            PivotItem p = MainPivot.SelectedItem as PivotItem;
                                            Frame f = p.Content as Frame;
                                            IndexPage page = f.Content as IndexPage;
                                            page.ChangeTabView(pageUrl);
                                        };
                                        flyout.Items.Add(menuFlyoutItem);
                                    }
                                MenuFlyoutItem flyoutItem = flyout.Items[0] as MenuFlyoutItem;
                                flyoutItem.Icon = new SymbolIcon(Symbol.Accept);
                                button.Flyout = flyout;
                                StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
                                stackPanel.Children.Add(new TextBlock { Text = flyoutItem.Text, Name = "TitleTextBlock" });
                                stackPanel.Children.Add(button);
                                i.Header = stackPanel;
                                i.Name = "FollowPivot";
                            }
                            pivot.Items.Add(i);
                        }
                        pivot.SelectedIndex = 1;
                    }
                }
            }
            PivotItem pivotItem = e.AddedItems[0] as PivotItem;
            Frame frame = pivotItem.Content as Frame;
            if (!(frame is null) && !frame.CanGoBack)
                frame.Navigate(typeof(IndexPage), new object[] { mainPage, pivotItem.Tag, true });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            PivotItem pivotItem = MainPivot.SelectedItem as PivotItem;
            Frame frame = pivotItem.Content as Frame;
            IndexPage page = frame.Content as IndexPage;
            page.RefreshPage();
        }
    }
}
