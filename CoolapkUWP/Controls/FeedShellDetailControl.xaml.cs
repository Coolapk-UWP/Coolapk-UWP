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

        public event EventHandler<string> ComboBoxSelectionChanged;

        public event EventHandler RequireRefresh;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = sender as ComboBox;
            string answerSortType = string.Empty;
            switch (box.SelectedIndex)
            {
                case -1: return;
                case 0:
                    answerSortType = "reply";
                    break;

                case 1:
                    answerSortType = "like";
                    break;

                case 2:
                    answerSortType = "dateline";
                    break;
            }
            ComboBoxSelectionChanged?.Invoke(this, answerSortType);
        }

        private void Image_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.ShowImage((sender as FrameworkElement).Tag as Models.ImageModel);

        private void StackPanel_Tapped(object sender, TappedRoutedEventArgs e) => UIHelper.OpenLinkAsync((sender as FrameworkElement).Tag as string);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.OpenLinkAsync((sender as Button).Tag as string);
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if ((sender as FrameworkElement).Tag is string s) UIHelper.OpenLinkAsync(s);
        }

        private void makeFeed_MakedFeedSuccessful(object sender, EventArgs e)
        {
            RequireRefresh?.Invoke(this, new EventArgs());
        }
    }
}