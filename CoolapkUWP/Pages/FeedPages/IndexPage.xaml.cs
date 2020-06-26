using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class IndexPage : Page
    {
        private ViewModels.IndexPage.ViewModel provider;

        public IndexPage() => this.InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            provider = e.Parameter as ViewModels.IndexPage.ViewModel;
            if (!provider.ShowTitleBar)
            {
                titleBar.Visibility = Visibility.Collapsed;
                listBorder.Padding = new Thickness(0);
            }

            ShowProgressRing();
            listView.ItemsSource = provider.mainModels;
            await Refresh();

            await Task.Delay(30);
            titleBar.Title = provider.Title;
            scrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);

            HideProgressRing();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            provider.VerticalOffsets[0] = scrollViewer.VerticalOffset;
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private void ShowProgressRing()
        {
            if (provider?.ShowTitleBar ?? false)
            {
                titleBar.ShowProgressRing();
            }
            else
            {
                UIHelper.ShowMainPageProgressRing();
            }
        }

        private void HideProgressRing()
        {
            if (provider?.ShowTitleBar ?? false)
            {
                titleBar.HideProgressRing();
            }
            else
            {
                UIHelper.HideMainPageProgressRing();
            }
        }

        private async Task Refresh(int p = -1)
        {
            ShowProgressRing();
            if (p == 1)
            {
                scrollViewer.ChangeView(null, 0, null);
            }
            await provider?.Refresh(p);
            HideProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                _ = Refresh();
            }
        }

        public void RefreshPage()
        {
            _ = Refresh(1);
        }

        private void TitleBar_BackButtonClick(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void OnTapped(object tag)
        {
            if (tag is string s)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    UIHelper.OpenLinkAsync(s);
                }
            }
            else if (tag is IHasUriAndTitle u)
            {
                if (string.IsNullOrEmpty(u.Url) || u.Url == "/topic/quickList?quickType=list") { return; }
                string str = u.Url;
                if (str == "Refresh") { RefreshPage(); }
                else if (str == "Login")
                {
                    UIHelper.Navigate(typeof(BrowserPage), new object[] { true, null });
                }
                else if (str.IndexOf("/page", StringComparison.Ordinal) == 0)
                {
                    str = str.Replace("/page", "/page/dataList", StringComparison.Ordinal);
                    str += $"&title={u.Title}";
                    UIHelper.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel(str, true));
                }
                else if (str.IndexOf('#') == 0)
                {
                    UIHelper.Navigate(typeof(IndexPage), new ViewModels.IndexPage.ViewModel($"{str}&title={u.Title}", true));
                }
                else { UIHelper.OpenLinkAsync(str); }
            }
            else if (tag is IndexPageModel i && !string.IsNullOrEmpty(i.Url))
            {
                UIHelper.OpenLinkAsync(i.Url);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (e != null && !UIHelper.IsOriginSource(sender, e.OriginalSource)) { return; }

            OnTapped((sender as FrameworkElement).Tag);
        }

        private void ListViewItem_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                OnTapped((sender as FrameworkElement).Tag);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OnTapped((sender as FrameworkElement).Tag);
        }

        private void ListViewItem_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            var model = (sender as FrameworkElement).DataContext as IndexPageModel;
            var u = $"/page/dataList?url={model.Url.Replace("#", "%23", StringComparison.Ordinal)}&title={model.Title}";
            if (provider.tabProviders.Count > 0)
            {
                provider.tabProviders[provider.ComboBoxSelectedIndex].ChangeGetDataFunc(
                    ViewModels.IndexPage.ViewModel.GetData(u, false),
                    (a) => a.EntityType == "feed");
            }
            else
            {
                provider.mainProvider.ChangeGetDataFunc(
                    ViewModels.IndexPage.ViewModel.GetData(u, false),
                    (a) => a.EntityType == "feed");
            }
            _ = Refresh();
        }

        private void FlipView_Loaded(object sender, RoutedEventArgs e)
        {
            var view = sender as FlipView;
            var timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(4)
            };
            timer.Tick += (o, a) =>
            {
                if (view.SelectedIndex + 1 >= view.Items.Count())
                {
                    while (view.SelectedIndex > 0)
                    {
                        view.SelectedIndex -= 1;
                    }
                }
                else
                {
                    view.SelectedIndex += 1;
                }
            };

            timer.Start();
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.Items.Count == 0)
            {
                foreach (IndexPageModel model in pivot.Tag as System.Collections.IEnumerable)
                {
                    provider.AddTab($"/page/dataList?url={model.Url.Replace("#", "%23", StringComparison.Ordinal)}&title={model.Title}");

                    var list = new Microsoft.UI.Xaml.Controls.ItemsRepeater
                    {
                        ItemTemplate = Resources["FTemplateSelector"] as DataTemplateSelector,
                        ItemsSource = provider.tabProviders.Last().Models,
                    };

                    var pivotItem = new PivotItem
                    {
                        Content = list,
                        Header = model.Title
                    };
                    pivot.Items.Add(pivotItem);
                }
                return;
            }
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Pivot element = sender as Pivot;
            if (element.SelectedIndex == -1) { return; }

            ShowProgressRing();
            await provider.SetComboBoxSelectedIndex(element.SelectedIndex);
            HideProgressRing();
        }
    }
}