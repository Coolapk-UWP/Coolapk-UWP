using CoolapkUWP.ViewModels.FeedPages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FindPage : Page
    {
        private static int PivotIndex = 0;

        private Action Refresh;

        public FindPage() => InitializeComponent();

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            PivotIndex = Pivot.SelectedIndex;
        }

        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
            Pivot.ItemsSource = GetMainItems();
            Pivot.SelectedIndex = PivotIndex;
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PivotItem MenuItem = Pivot.SelectedItem as PivotItem;
            if ((Pivot.SelectedItem as PivotItem).Content is Frame Frame && Frame.Content is null)
            {
                string url = MenuItem.Tag.ToString() == "V9_HOME_TAB_HEADLINE"
                    ? "/main/indexV8"
                    : MenuItem.Tag.ToString() == "V11_FIND_DYH"
                        ? "/user/dyhSubscribe"
                        : $"/page?url={MenuItem.Tag}";
                _ = Frame.Navigate(typeof(AdaptivePage), new AdaptiveViewModel(url));
                Refresh = () => _ = (Frame.Content as AdaptivePage).Refresh(true);
            }
            else if ((Pivot.SelectedItem as PivotItem).Content is Frame __ && __.Content is AdaptivePage AdaptivePage)
            {
                Refresh = () => _ = AdaptivePage.Refresh(true);
            }
        }

        public static ObservableCollection<PivotItem> GetMainItems()
        {
            ResourceLoader loader = ResourceLoader.GetForCurrentView("FindPage");
            ObservableCollection<PivotItem> items = new ObservableCollection<PivotItem>
            {
                new PivotItem() { Tag = "V11_HOME_NEW", Header = loader.GetString("V11_HOME_NEW"), Content = new Frame() },
                new PivotItem() { Tag = "V9_HOME_TAB_SHIPIN", Header = loader.GetString("V9_HOME_TAB_SHIPIN"), Content = new Frame() },
                new PivotItem() { Tag = "V11_HOME_CAR", Header = loader.GetString("V11_HOME_CAR"), Content = new Frame() },
                new PivotItem() { Tag = "V10_DIGITAL_HOME", Header = loader.GetString("V10_DIGITAL_HOME"), Content = new Frame() },
                new PivotItem() { Tag = "V10_CHANNEL_SJB", Header = loader.GetString("V10_CHANNEL_SJB"), Content = new Frame() },
                new PivotItem() { Tag = "V11_ZHUANTI_EARPHONE", Header = loader.GetString("V11_ZHUANTI_EARPHONE"), Content = new Frame() },
                new PivotItem() { Tag = "V11_FIND_GOOD_GOODS_HOME", Header = loader.GetString("V11_FIND_GOOD_GOODS_HOME"), Content = new Frame() },
            };
            return items;
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e) => Refresh();
    }
}
