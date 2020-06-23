using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
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
        private readonly ObservableCollection<FeedModel> answers = new ObservableCollection<FeedModel>();
        private int answersPage;
        private double answerFirstItem, answerLastItem;
        private string answerSortType = "reply";
        private readonly string[] comboBoxItems = new string[] { "最近回复", "按时间排序", "按热度排序", "只看楼主" };

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

        public FeedShellPage() => this.InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            feedId = e.Parameter as string;
            LoadFeedDetail();
        }

        public async void LoadFeedDetail()
        {
            TitleBar.ShowProgressRing();
            if (string.IsNullOrEmpty(feedId)) return;
            JObject detail = (JObject)await DataHelper.GetDataAsync(DataUriType.GetFeedDetail, feedId);
            if (detail != null)
            {
                FeedDetail = new FeedDetailModel(detail);
                TitleBar.Title = FeedDetail.Title;
                if (FeedDetail.IsQuestionFeed)
                {
                    if (answersPage != 0 || answerLastItem != 0) return;
                    FindName(nameof(AnswersListView));
                    PivotItemPanel.Visibility = Visibility.Collapsed;
                    RefreshAnswers();
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

                    listCtrl.RefreshHotReplies();
                    listCtrl.RefreshFeedReplies();
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
                    if (FeedDetail.Liked)
                    {
                        JObject o = (JObject)await DataHelper.GetDataAsync(DataUriType.OperateUnlike, string.Empty, feedId);
                        if (o != null)
                        {
                            FeedDetail.Likenum = $"{o.Value<int>("count")}";
                            like1.Visibility = Visibility.Collapsed;
                            like2.Visibility = Visibility.Visible;
                            FeedDetail.Liked = false;
                        }
                    }
                    else
                    {
                        JObject o = (JObject)await DataHelper.GetDataAsync(DataUriType.OperateLike, string.Empty, feedId);
                        if (o != null)
                        {
                            FeedDetail.Likenum = $"{o.Value<int>("count")}";
                            like1.Visibility = Visibility.Visible;
                            like2.Visibility = Visibility.Collapsed;
                            FeedDetail.Liked = true;
                        }
                    }
                    break;

                default:
                    UIHelper.OpenLinkAsync(tag);
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private async void Refresh()
        {
            TitleBar.ShowProgressRing();
            if (listCtrl != null)
            {
                listCtrl.Refresh();
            }
            else if (AnswersListView != null)
            {
                if (answerLastItem != 0) { return; }
                RefreshAnswers(1);
            }
            else
            {
                LoadFeedDetail();
                TitleBar.HideProgressRing();
                return;
            }
            if (RefreshAll) await RefreshFeedDetail();
            TitleBar.HideProgressRing();
        }

        private async Task RefreshFeedDetail()
        {
            JObject detail = (JObject)await DataHelper.GetDataAsync(DataUriType.GetFeedDetail, feedId);
            if (detail != null) FeedDetail = new FeedDetailModel(detail);
        }

        private void detailControl_ComboBoxSelectionChanged(object sender, string e)
        {
            answerSortType = e;
            answers.Clear();
            answerFirstItem = answerLastItem = 0;
            answersPage = 0;
            Refresh();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
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
                        listCtrl.Refresh();
                    }
                    else if (AnswersListView != null)
                    {
                        RefreshAnswers();
                    }

                    TitleBar.HideProgressRing();
                }
            }
        }

        private void ChangeFeedSortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!FeedDetail.IsQuestionFeed && rightComboBox.SelectedIndex != -1)
            {
                listCtrl.ChangeFeedSorting(rightComboBox.SelectedIndex);
            }
        }

        private void detailControl_RequireRefresh(object sender, EventArgs e)
        {
            rightComboBox.SelectedIndex = 1;
        }

        private async void RefreshAnswers(int p = -1)
        {
            JArray array = (JArray)await DataHelper.GetDataAsync(
                                        DataUriType.GetAnswers,
                                        feedId,
                                        answerSortType,
                                        p == -1 ? ++answersPage : p,
                                        answerFirstItem == 0 ? string.Empty : $"&firstItem={answerFirstItem}",
                                        answerLastItem == 0 ? string.Empty : $"&lastItem={answerLastItem}");
            if (array != null && array.Count != 0)
            {
                if (p == 1 || answersPage == 1)
                {
                    var d = (from a in answers
                             from b in array
                             where a.Url == b.Value<string>("url")
                             select a).ToArray();
                    foreach (var item in d)
                        answers.Remove(item);
                    for (int i = 0; i < array.Count; i++)
                        answers.Insert(i, new FeedModel((JObject)array[i], FeedDisplayMode.notShowMessageTitle));
                    answerFirstItem = array.First.Value<int>("id");
                    if (answersPage == 1)
                        answerLastItem = array.Last.Value<int>("id");
                }
                else
                {
                    answerLastItem = array.Last.Value<int>("id");
                    foreach (JObject item in array)
                        answers.Add(new FeedModel(item, FeedDisplayMode.notShowMessageTitle));
                }
            }
            else if (p == -1)
            {
                answersPage--;
                UIHelper.ShowMessage("没有更多回答了");
            }
        }

        #region 界面模式切换

        private bool RefreshAll;
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
                RefreshAll = false;
            }

            if ((e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 768 && !((FeedDetail?.IsFeedArticle ?? false) || (FeedDetail?.IsCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(384);
                SetDualPanelMode();
                PivotItemPanel.Width = 384;
                PivotItemPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if ((e?.NewSize.Width ?? Window.Current.Bounds.Width) >= 832 && ((FeedDetail?.IsFeedArticle ?? false) || (FeedDetail?.IsCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(480);
                SetDualPanelMode();
                PivotItemPanel.Width = 480;
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
                RefreshAll = true;
            }
        }

        #endregion 界面模式切换
    }
}