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
                            PivotItem i = new PivotItem
                            {
                                Header = it["title"].ToString(),
                                Tag = it["title"].ToString() == "头条" ? string.Empty : it["url"].ToString() + "&title=" + it["title"].ToString(),
                                Content = new Frame()
                            };
                            pivot.Items.Add(i);
                        }
                        pivot.SelectedIndex = 1;
                    }
                }
            }
            PivotItem pivotItem = e.AddedItems[0] as PivotItem;
            Frame frame = pivotItem.Content as Frame;
            if (!(frame is null) && !frame.CanGoBack)
                frame.Navigate(typeof(IndexPage), new object[] { mainPage, pivotItem.Tag ,true});
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
