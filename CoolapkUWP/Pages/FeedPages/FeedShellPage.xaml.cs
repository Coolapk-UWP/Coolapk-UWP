using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using CoolapkUWP.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedShellPage : Page, INotifyPropertyChanged
    {
        private string feedId;
        private FeedDetailModel feedDetail;
        private FeedListProvider answerProvider;
        private string answerSortType = "reply";
        private FeedDetailModel FeedDetail
        {
            get => feedDetail;
            set
            {
                feedDetail = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public FeedShellPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if(FeedDetail != null)
            {
                FeedDetail.IsCopyEnabled = false;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            feedId = e.Parameter as string;
            LoadFeedDetail();
        }

        private void MoveToTop()
        {
            if (MainScrollMode == ScrollMode.Disabled)
            {
                rightScrollViewer.ChangeView(null, 0, null);
            }
            else
            {
                var point = rightScrollViewer.TransformToVisual(mainScrollViewer).TransformPoint(new Windows.Foundation.Point(0, 0));
                System.Diagnostics.Debug.WriteLine(point.Y);
                mainScrollViewer.ChangeView(null, point.Y, null);
            }
        }

        private void listCtrl_NeedMoveToTop(object sender, EventArgs e)
        {
            MoveToTop();
        }

        public async Task LoadFeedDetail()
        {
            TitleBar.ShowProgressRing();
            if (string.IsNullOrEmpty(feedId)) { return; }
            var loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedShellPage");

            var detail = (JObject)await DataHelper.GetDataAsync(DataUriType.GetFeedDetail, feedId);
            if (detail != null)
            {
                FeedDetail = new FeedDetailModel(detail);
                TitleBar.Title = FeedDetail.Title;
                if (FeedDetail.IsQuestionFeed)
                {
                    if (answerProvider != null) { return; }

                    answerProvider =
                        new FeedListProvider(
                            async (p, page, firstItem, lastItem) =>
                                (JArray)await DataHelper.GetDataAsync(
                                    DataUriType.GetAnswers,
                                    feedId,
                                    answerSortType,
                                    p == -1 ? ++page : p,
                                    firstItem == 0 ? string.Empty : $"&firstItem={firstItem}",
                                    lastItem == 0 ? string.Empty : $"&lastItem={lastItem}"),
                            (a, b) => ((FeedModel)a).Url == b.Value<string>("url"),
                            (o) => new FeedModel(o, FeedDisplayMode.notShowMessageTitle),
                            () => Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("NotificationsPage").GetString("noMore"),
                            () => MoveToTop(),
                            "id");

                    FindName(nameof(answersList));

                    answersList.ItemsSource = answerProvider.Models;
                    PivotItemPanel.Visibility = Visibility.Collapsed;
                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("popular"),
                        loader.GetString("like"),
                        loader.GetString("dateline_desc"),
                    };

                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.SelectedIndex = 0;
                }
                else
                {
                    FindName(nameof(listCtrl));
                    listCtrl.FeedId = feedId;
                    if (detailControl.FeedArticleTitle != null)
                    {
                        detailControl.FeedArticleTitle.Height = detailControl.FeedArticleTitle.Width * 0.44;
                        Page_SizeChanged(null, null);
                    }
                    else if (FeedDetail.IsCoolPictuers)
                    {
                        Page_SizeChanged(null, null);
                    }

                    rightComboBox.ItemsSource = new string[]
                    {
                        loader.GetString("lastupdate_desc"),
                        loader.GetString("dateline_desc"),
                        loader.GetString("popular"),
                        loader.GetString("isFromAuthor"),
                    };
                    rightComboBox.Visibility = Visibility.Visible;
                    rightComboBox.SelectedIndex = 0;
                }
            }
            if (feedDetail.SourceFeed?.ShowPicArr ?? false)
            {
                FindName("sourcePic");
            }

            TitleBar.HideProgressRing();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag as string;
            switch (tag)
            {
                case "reply":
                    listCtrl.ChangeSelection(0);
                    ToReplyPivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToLikePivotItemButton.BorderThickness = ToSharePivotItemButton.BorderThickness = new Thickness(0);
                    rightComboBox.Visibility = Visibility.Visible;
                    break;

                case "like":
                    listCtrl.ChangeSelection(1);
                    ToLikePivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToReplyPivotItemButton.BorderThickness = ToSharePivotItemButton.BorderThickness = new Thickness(0);
                    rightComboBox.Visibility = Visibility.Collapsed;
                    break;

                case "share":
                    listCtrl.ChangeSelection(2);
                    ToSharePivotItemButton.BorderThickness = new Thickness(0, 0, 0, 2);
                    ToReplyPivotItemButton.BorderThickness = ToLikePivotItemButton.BorderThickness = new Thickness(0);
                    rightComboBox.Visibility = Visibility.Collapsed;
                    break;

                case "MakeLike":
                    await DataHelper.MakeLikeAsync(FeedDetail, Dispatcher, like1, like2);
                    break;

                default:
                    UIHelper.OpenLinkAsync(tag);
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private void listCtrl_NeedProgressRing(object sender, bool e)
        {
            if (e)
            {
                TitleBar.ShowProgressRing();
            }
            else
            {
                TitleBar.HideProgressRing();
            }
        }

        private async void Refresh()
        {
            TitleBar.ShowProgressRing();

            await RefreshFeedDetail();
            if (listCtrl != null)
            {
                await listCtrl.Refresh();
            }
            else if (answersList != null)
            {
                await answerProvider.Refresh(1);
            }
            else
            {
                await LoadFeedDetail();
            }

            TitleBar.HideProgressRing();
        }

        private async Task RefreshFeedDetail()
        {
            var detail = (JObject)await DataHelper.GetDataAsync(DataUriType.GetFeedDetail, feedId);
            if (detail != null)
            {
                FeedDetail = new FeedDetailModel(detail);
            }
        }

        private async void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate)
            {
                double a = scrollViewer.VerticalOffset;
                if (a == scrollViewer.ScrollableHeight)
                {
                    TitleBar.ShowProgressRing();
                    scrollViewer.ChangeView(null, a, null);
                    if (listCtrl != null)
                    {
                        await listCtrl.Refresh(false);
                    }
                    else if (answersList != null)
                    {
                        await answerProvider.Refresh();
                    }

                    TitleBar.HideProgressRing();
                }
            }
        }

        private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightComboBox.SelectedIndex != -1)
            {
                if (FeedDetail.IsQuestionFeed)
                {
                    switch (rightComboBox.SelectedIndex)
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

                    answerProvider?.Reset();
                    Refresh();
                }
                else
                {
                    listCtrl.ChangeFeedSorting(rightComboBox.SelectedIndex);
                }
            }
        }

        private void detailControl_RequireRefresh(object sender, EventArgs e)
        {
            rightComboBox.SelectedIndex = 1;
        }

        #region 界面模式切换

        private double detailListHeight;
        private ScrollMode mainScrollMode = ScrollMode.Auto;

        private double DetailListHeight
        {
            get => detailListHeight;
            set
            {
                detailListHeight = value;
                RaisePropertyChangedEvent();
            }
        }

        public ScrollMode MainScrollMode
        {
            get => mainScrollMode;
            set
            {
                mainScrollMode = value;
                RaisePropertyChangedEvent();
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (detailControl.FeedArticleTitle != null)
            {
                detailControl.FeedArticleTitle.Height = detailControl.FeedArticleTitle.Width * 0.44;
            }

            void SetDualPanelMode()
            {
                MainScrollMode = ScrollMode.Disabled;
                DetailListHeight = e?.NewSize.Height ?? Window.Current.Bounds.Height;

                RightColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
                detailBorder.Padding = new Thickness(0, (double)Windows.UI.Xaml.Application.Current.Resources["PageTitleHeight"], 0, PivotItemPanel.ActualHeight + 16);
                detailBorder.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                mainGrid.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                mainGrid.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                mainGrid.Padding = new Thickness(0);
                mainGrid.Margin = new Thickness(0);
                rightScrollViewer.SetValue(Grid.ColumnProperty, 1);
                rightScrollViewer.SetValue(Grid.RowProperty, 0);
                rightGrid.Padding = (Thickness)Windows.UI.Xaml.Application.Current.Resources["StackPanelMargin"];
                rightGrid.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                rightGrid.InvalidateArrange();
            }

            if ((e?.NewSize.Width ?? Width) >= 804 && !((FeedDetail?.IsFeedArticle ?? false) || (FeedDetail?.IsCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(420);
                SetDualPanelMode();
                PivotItemPanel.Width = 420;
                PivotItemPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if ((e?.NewSize.Width ?? Width) >= 876 && ((FeedDetail?.IsFeedArticle ?? false) || (FeedDetail?.IsCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(520);
                SetDualPanelMode();
                PivotItemPanel.Width = 520;
                PivotItemPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                MainScrollMode = ScrollMode.Auto;
                DetailListHeight = double.NaN;

                mainGrid.Margin = new Thickness(0, 0, 0, PivotItemPanel.ActualHeight);
                mainGrid.Padding = (Thickness)Windows.UI.Xaml.Application.Current.Resources["StackPanelMargin"];
                mainGrid.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Auto);
                mainGrid.SetValue(ScrollViewer.VerticalScrollBarVisibilityProperty, ScrollBarVisibility.Auto);
                PivotItemPanel.Width = double.NaN;
                PivotItemPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                detailBorder.Padding = rightGrid.Padding = new Thickness(0, 0, 0, 12);
                detailBorder.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
                LeftColumnDefinition.Width = new GridLength(1, GridUnitType.Star);
                RightColumnDefinition.Width = new GridLength(0);
                rightScrollViewer.SetValue(Grid.ColumnProperty, 0);
                rightScrollViewer.SetValue(Grid.RowProperty, 1);
                rightGrid.SetValue(ScrollViewer.VerticalScrollModeProperty, ScrollMode.Disabled);
            }
        }

        #endregion 界面模式切换
    }
}