using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace CoolapkUWP.Controls
{
    public sealed partial class FeedShellDetailControl : UserControl, INotifyPropertyChanged
    {
        private FeedDetailModel feedDetail;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FeedDetailModel FeedDetail
        {
            get => feedDetail;
            set
            {
                feedDetail = value;
                if (!(value?.IsQuestionFeed ?? true))
                {
                    FindName(nameof(makeFeed));
                }
                RaisePropertyChangedEvent();
            }
        }

        internal Grid FeedArticleTitle { get => feedArticleTitle; }

        public FeedShellDetailControl()
        {
            this.InitializeComponent();
        }

        public event EventHandler RequireRefresh;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.ShowImage((sender as FrameworkElement).Tag as Models.ImageModel);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == reportButton)
            {
                FeedDetail.IsCopyEnabled = false;
                UIHelper.Navigate(typeof(Pages.BrowserPage), new object[] { false, $"https://m.coolapk.com/mp/do?c=feed&m=report&type=feed&id={FeedDetail.Id}" });
            }
            else
            {
                UIHelper.OpenLinkAsync((sender as Button).Tag as string);
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is string s) UIHelper.OpenLinkAsync(s);
        }

        private void makeFeed_MakedFeedSuccessful(object sender, EventArgs e)
        {
            RequireRefresh?.Invoke(this, new EventArgs());
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            moreButton.Flyout.ShowAt(this);
        }

        private void Grid_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Menu)
            {
                moreButton.Flyout.ShowAt(this);
            }
        }
    }
}