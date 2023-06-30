using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using CoolapkUWP.ViewModels;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ShowImagePage : Page
    {
        private Point _clickPoint = new Point(0, 0);
        private ShowImageViewModel Provider;

        public ShowImagePage() => InitializeComponent();

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is ImageModel Model)
            {
                Provider = new ShowImageViewModel(Model, Dispatcher);
                DataContext = Provider;
            }
            else if (e.Parameter is ShowImageViewModel ViewModel)
            {
                Provider = ViewModel;
                DataContext = Provider;
            }
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed += System_BackPressed; }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Changed -= AppWindow_Changed;
            }
            else
            {
                Window.Current?.SetTitleBar(null);
                SystemNavigationManager.GetForCurrentView().BackRequested -= System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                TitleBar.LayoutMetricsChanged -= TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged -= TitleBar_IsVisibleChanged;
                Frame.Navigated -= On_Navigated;
            }
            if (ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
            { HardwareButtons.BackPressed -= System_BackPressed; }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsAppWindow())
            {
                this.GetWindowForElement().Changed += AppWindow_Changed;
            }
            else
            {
                Window.Current.SetTitleBar(CustomTitleBar);
                if (ApiInformation.IsMethodPresent("Windows.UI.Composition.Compositor", "TryCreateBlurredWallpaperBackdropBrush"))
                { BackdropMaterial.SetApplyToRootOrPageBackground(this, true); }
                SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack(false);
                SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
                CoreApplicationViewTitleBar TitleBar = CoreApplication.GetCurrentView().TitleBar;
                if (!(AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop"))
                { UpdateContentLayout(TitleBar); }
                TitleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
                TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
                Frame.Navigated += On_Navigated;
                UpdateTitleBarLayout(TitleBar);
            }
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = TryGoBack();
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack() == AppViewBackButtonVisibility.Visible;
            }
        }

        private void System_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (!e.Handled)
            {
                e.Handled = TryGoBack() == AppViewBackButtonVisibility.Visible;
            }
        }

        private void AppWindow_Changed(AppWindow sender, AppWindowChangedEventArgs args)
        {
            bool IsVisible = sender.TitleBar.IsVisible;
            CustomTitleBar.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private AppViewBackButtonVisibility TryGoBack(bool goBack = true)
        {
            if (!Dispatcher.HasThreadAccess || !Frame.CanGoBack)
            { return AppViewBackButtonVisibility.Collapsed; }

            if (goBack) { Frame.GoBack(); }
            return AppViewBackButtonVisibility.Visible;
        }

        private void UpdateContentLayout(CoreApplicationViewTitleBar TitleBar)
        {
            bool IsVisible = TitleBar.IsVisible;
            CustomTitleBar.Visibility = IsVisible ? Visibility.Visible : Visibility.Collapsed;
            Thickness Margin = new Thickness(0, IsVisible ? TitleBar.Height : 0, 0, 0);
            FlipViewGrid.Margin = Margin;
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar TitleBar)
        {
            LeftPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayLeftInset);
            RightPaddingColumn.Width = new GridLength(TitleBar.SystemOverlayRightInset);
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag as string)
            {
                case "Copy":
                    Provider.CopyPic();
                    break;
                case "Save":
                    Provider.SavePic();
                    break;
                case "Share":
                    Provider.SharePic();
                    break;
                case "Refresh":
                    _ = Provider.Refresh();
                    break;
                case "Origin":
                    Provider.Images[Provider.Index].Type &= (ImageType)0xFE;
                    Provider.ShowOrigin = false;
                    break;
            }
        }

        private void ScrollViewer_Tapped(object sender, TappedRoutedEventArgs e)
        {
            CommandBar.Visibility = CommandBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ScrollViewer_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            Point doubleTapPoint = e.GetPosition(scrollViewer);

            _ = scrollViewer.ZoomFactor != 1
                ? scrollViewer.ChangeView(0, 0, 1)
                : scrollViewer.ChangeView(doubleTapPoint.X, doubleTapPoint.Y, 2);

            CommandBar.Visibility = CommandBar.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            FrameworkElement element = scrollViewer.Content as FrameworkElement;
            element.CanDrag = scrollViewer.ZoomFactor <= 1;
        }

        private async void Image_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
            args.DragUI.SetContentFromDataPackage();
            args.Data.RequestedOperation = DataPackageOperation.Copy;
            await Provider.GetImageDataPackage(args.Data, "拖拽图片");
        }

        private void Image_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                _clickPoint = e.GetCurrentPoint(element).Position;
            }
        }

        private void Image_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ScrollViewer scrollViewer = element.Parent as ScrollViewer;
            PointerPoint pointerPoint = e.GetCurrentPoint(element);
            if (pointerPoint.Properties.IsLeftButtonPressed)
            {
                double x, y;
                Point point = e.GetCurrentPoint(element).Position;
                x = _clickPoint.X - point.X;
                y = _clickPoint.Y - point.Y;
                _ = scrollViewer.ChangeView(scrollViewer.HorizontalOffset + x, scrollViewer.VerticalOffset + y, null);
            }
        }

        private void Image_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ScrollViewer view = (sender as FrameworkElement).Parent as ScrollViewer;
            _ = view.ChangeView(view.HorizontalOffset - (e.Delta.Translation.X * view.ZoomFactor), view.VerticalOffset - (e.Delta.Translation.Y * view.ZoomFactor), null);
        }

        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) => UpdateContentLayout(sender);

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args) => UpdateTitleBarLayout(sender);
    }
}
