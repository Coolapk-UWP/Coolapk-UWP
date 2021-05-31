using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.FeedDetailPage;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CoolapkUWP.Pages.FeedPages
{
    public sealed partial class FeedShellPage : Page, INotifyPropertyChanged
    {
        private ViewModel provider;

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public FeedShellPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            provider = e.Parameter as ViewModel;
            provider.PropertyChanged += Provider_PropertyChanged;
            Refresh(-2);
            if (provider.FeedDetail != null)
            {
                SetLayout();
            }

            await System.Threading.Tasks.Task.Delay(30);

            titleBar.Title = provider.Title;
            if (MainScrollMode == ScrollMode.Disabled)
            {
                _ = detailScrollViewer.ChangeView(null, provider.VerticalOffsets[1], null, true);
                _ = rightScrollViewer.ChangeView(null, provider.VerticalOffsets[2], null, true);
            }
            else
            {
                _ = mainScrollViewer.ChangeView(null, provider.VerticalOffsets[0], null, true);
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (provider.FeedDetail != null)
            {
                provider.FeedDetail.IsCopyEnabled = false;
            }
            provider.PropertyChanged -= Provider_PropertyChanged;
            if (MainScrollMode == ScrollMode.Disabled)
            {
                provider.VerticalOffsets[1] = detailScrollViewer.VerticalOffset;
                provider.VerticalOffsets[2] = rightScrollViewer.VerticalOffset;
            }
            else
            {
                provider.VerticalOffsets[0] = mainScrollViewer.VerticalOffset;
            }
            titleBar.Title = string.Empty;

            base.OnNavigatingFrom(e);
        }

        private void Provider_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(provider.FeedDetail) && provider.FeedDetail != null)
            {
                SetLayout();
            }
        }

        private void SetLayout()
        {
            titleBar.ShowProgressRing();

            detailControl.FeedDetail = provider.FeedDetail;

            Windows.ApplicationModel.Resources.ResourceLoader loader = Windows.ApplicationModel.Resources.ResourceLoader.GetForViewIndependentUse("FeedShellPage");
            if (provider.FeedDetail.IsQuestionFeed)
            {
                if (listControl != null)
                {
                    UnloadObject(listControl);
                }
                _ = FindName(nameof(answersList));
                answersList.ItemsSource = ((QuestionViewModel)provider).Models;

                PivotItemPanel.Visibility = Visibility.Collapsed;
                rightComboBox.ItemsSource = new string[]
                {
                    loader.GetString("popular"),
                    loader.GetString("like"),
                    loader.GetString("dateline_desc"),
                };
                rightComboBox.SelectedIndex = ((QuestionViewModel)provider).ComboBoxSelectedIndex;
                rightComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                if (answersList != null)
                {
                    UnloadObject(answersList);
                }
                _ = FindName(nameof(listControl));
                FeedViewModel feedViewModel = provider as FeedViewModel;
                listControl.SetProvider(feedViewModel.ReplyListVM);

                PivotItemPanel.DataContext = provider.FeedDetail;
                PivotItemPanel.Visibility = Visibility.Visible;

                if (detailControl.FeedArticleTitle != null)
                {
                    detailControl.FeedArticleTitle.Height = detailControl.FeedArticleTitle.Width * 0.44;
                }

                rightComboBox.ItemsSource = new string[]
                {
                    loader.GetString("lastupdate_desc"),
                    loader.GetString("dateline_desc"),
                    loader.GetString("popular"),
                    loader.GetString("isFromAuthor"),
                };
                rightComboBox.SelectedIndex = feedViewModel.ReplyListVM?.ComboBoxSelectedIndex ?? 0;

                ChangeListControlSelection(feedViewModel.ReplyListVM.SelectedIndex);
            }

            if (provider.FeedDetail.SourceFeed?.ShowPicArr ?? false)
            {
                _ = FindName("sourcePic");
            }
            Page_SizeChanged(null, null);
            titleBar.Title = provider.Title;
            showQRCodeControl.QRCodeText = provider.FeedDetail.Shareurl;
        }

        private void MoveToTop()
        {
            if (MainScrollMode == ScrollMode.Disabled)
            {
                _ = rightScrollViewer.ChangeView(null, 0, null);
            }
            else
            {
                Windows.Foundation.Point point = rightScrollViewer.TransformToVisual(mainScrollViewer).TransformPoint(new Windows.Foundation.Point(0, 0));
                System.Diagnostics.Debug.WriteLine(point.Y);
                _ = mainScrollViewer.ChangeView(null, point.Y, null);
            }
        }

        private ImmutableArray<Button> pivotButtons;

        private void ChangeListControlSelection(int n)
        {
            if (n < 0 || n > 2) { return; }

            if (pivotButtons.IsDefaultOrEmpty)
            {
                pivotButtons = new Button[]
                {
                    toReplyPivotItemButton,
                    toLikePivotItemButton,
                    toSharePivotItemButton,
                }.ToImmutableArray();
            }

            listControl.ChangeSelection(n);

            rightComboBox.Visibility = n == 0 ? Visibility.Visible : Visibility.Collapsed;
            for (int i = 0; i < 3; i++)
            {
                pivotButtons[i].BorderThickness = i == n ? new Thickness(0, 0, 0, 2) : new Thickness(0);
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button).Tag as string;
            switch (tag)
            {
                case "reply":
                    ChangeListControlSelection(0);
                    break;

                case "like":
                    ChangeListControlSelection(1);
                    break;

                case "share":
                    ChangeListControlSelection(2);
                    break;

                case "MakeLike":
                    await DataHelper.MakeLikeAsync(provider.FeedDetail, Dispatcher, like1, like2);
                    break;

                default:
                    UIHelper.OpenLinkAsync(tag);
                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                Frame.GoBack();
            }
        }

        private void listCtrl_NeedProgressRing(object sender, bool e)
        {
            if (e)
            {
                titleBar.ShowProgressRing();
            }
            else
            {
                titleBar.HideProgressRing();
            }
        }

        private async void Refresh(int p = -1)
        {
            titleBar.ShowProgressRing();

            if (p == -2)
            {
                MoveToTop();
            }
            await provider.Refresh(p);

            titleBar.Title = provider.Title;
            titleBar.HideProgressRing();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (!e.IsIntermediate && scrollViewer.VerticalOffset == scrollViewer.ScrollableHeight)
            {
                Refresh();
            }
        }

        private void titleBar_RefreshButtonClicked(object sender, RoutedEventArgs e)
        {
            Refresh(-2);
        }

        private void rightComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (rightComboBox.SelectedIndex != -1)
            {
                if (provider is QuestionViewModel)
                {
                    ((ViewModels.ICanComboBoxChangeSelectedIndex)provider).SetComboBoxSelectedIndex(rightComboBox.SelectedIndex);
                }
                else
                {
                    listControl.ChangeFeedSorting(rightComboBox.SelectedIndex);
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

            if ((e?.NewSize.Width ?? ActualWidth) >= 804 && !((provider?.FeedDetail?.IsFeedArticle ?? false) || (provider.FeedDetail?.IsCoolPictuers ?? false)))
            {
                LeftColumnDefinition.Width = new GridLength(420);
                SetDualPanelMode();
                PivotItemPanel.Width = 420;
                PivotItemPanel.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else if ((e?.NewSize.Width ?? ActualWidth) >= 876 && ((provider.FeedDetail?.IsFeedArticle ?? false) || (provider.FeedDetail?.IsCoolPictuers ?? false)))
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