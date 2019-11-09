using CoolapkUWP.Data;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.FeedPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class InitialPage : Page
    {
        Grid grid = null;

        public InitialPage()
        {
            this.InitializeComponent();
            Task.Run(async () =>
            {
                await Task.Delay(100);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    FrameworkElement element = VisualTreeHelper.GetChild(MainPivot, 0) as FrameworkElement;
                    Grid a = VisualTreeHelper.GetChild(element, 1) as Grid;
                    ScrollViewer c = a.Children[0] as ScrollViewer;
                    FrameworkElement d = c.FindName("Panel") as FrameworkElement;
                    grid = d.FindName("HeaderGrid") as Grid;
                    grid.Height = Settings.FirstPageTitleHeight;
                });
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Tools.mainPage.ResetRowHeight();
            if (MainPivot.Items.Count == 0)
                Pivot_SelectionChanged(null, null);
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot pivot = MainPivot;
            if (pivot.Items.Count == 0)
            {
                string r = await Tools.GetJson("/main/init");
                JsonArray array = Tools.GetDataArray(r);
                if (array != null)
                    foreach (var o in array)
                    {
                        JsonObject item = o.GetObject();
                        if (item["entityTemplate"].GetString() == "configCard")
                        {
                            JsonArray jArray = item["entities"].GetArray();
                            foreach (var itemi in jArray)
                            {
                                JsonObject it = itemi.GetObject();
                                switch (itemi.GetObject()["title"].GetString())
                                {
                                    case "酷品":
                                    case "看看号":
                                    case "直播": continue;
                                }
                                PivotItem i = new PivotItem
                                {
                                    Header = it["title"].GetString(),
                                    ///main/indexV8 ? page=
                                    Tag = it["title"].GetString() == "头条" ? "/main/indexV8" : it["url"].GetString() + "&title=" + it["title"].GetString(),
                                    Content = new Frame()
                                };
                                if (it["title"].GetString() == "关注")
                                {
                                    Button button = new Button { Content = new TextBlock { Text = "▼" }, Background = null, Name = "moreB" };
                                    MenuFlyout flyout = new MenuFlyout();
                                    foreach (var t in it["entities"].GetArray())
                                    {
                                        JsonObject ite = t.GetObject();
                                        if (ite["entityType"].GetString() == "page")
                                        {
                                            MenuFlyoutItem menuFlyoutItem = new MenuFlyoutItem { Text = ite["title"].GetString(), Tag = ite["url"].GetString() + "&title=" + ite["title"].GetString() };
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
            if ((MainPivot.SelectedItem is PivotItem pivotItem) && pivotItem.Content is Frame frame && !frame.CanGoBack)
                frame.Navigate(typeof(IndexPage), new object[] { pivotItem.Tag, true ,this});
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (MainPivot.Items.Count == 0)
                Pivot_SelectionChanged(null, null);
            else if (MainPivot.SelectedItem is PivotItem pivotItem)
            {
                Frame frame = pivotItem.Content as Frame;
                IndexPage page = frame.Content as IndexPage;
                page.RefreshPage();
            }
        }

        public double ChangeRowHeight(double height)
        {
            if (height < 0 && height < Settings.FirstPageTitleHeight - 48 - grid.ActualHeight) height = Settings.FirstPageTitleHeight - 48 - grid.ActualHeight;
            if ((grid.ActualHeight + height > Settings.FirstPageTitleHeight) ||
                (grid.ActualHeight + height < Settings.FirstPageTitleHeight - 48)) return grid.Height;
            grid.Height += height;
            return grid.Height;
        }
    }
}
