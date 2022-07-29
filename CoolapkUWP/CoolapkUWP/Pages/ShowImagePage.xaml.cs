using CoolapkUWP.Helpers;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using InAppNotify = Microsoft.Toolkit.Uwp.UI.Controls.InAppNotification;

namespace CoolapkUWP.Pages
{
    internal class ImageModel : INotifyPropertyChanged
    {
        private bool isProgressRingActived;
        private WeakReference<BitmapImage> pic;
        private readonly InAppNotify inAppNotify;
        private readonly CoreDispatcher dispatcher;

        public string Uri { get; }
        public ImageType Type { get; private set; }
        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToUpperInvariant().Contains("GIF", StringComparison.Ordinal); }

        public BitmapImage Pic
        {
            get
            {
                if (pic != null && pic.TryGetTarget(out BitmapImage image))
                {
                    return image;
                }
                else
                {
                    GetImage();
                    return ImageCacheHelper.NoPic;
                }
            }
            private set
            {
                if (pic == null)
                {
                    pic = new WeakReference<BitmapImage>(value);
                }
                else
                {
                    pic.SetTarget(value);
                }
                RaisePropertyChangedEvent();
            }
        }

        public bool IsProgressRingActived
        {
            get => isProgressRingActived;
            set
            {
                isProgressRingActived = value;
                RaisePropertyChangedEvent();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (name != null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                }
            });
        }

        public ImageModel(string uri, ImageType type, InAppNotify notify, CoreDispatcher dispatcher)
        {
            Uri = uri;
            Type = IsGif || SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture) ? ChangeType(type) : type;
            inAppNotify = notify;
            this.dispatcher = dispatcher;
        }

        public ImageModel(Models.ImageModel model, InAppNotify notify, CoreDispatcher dispatcher) : this(model.Uri, model.Type, notify, dispatcher)
        {
        }

        private async void GetImage()
        {
            BitmapImage bitmapImage = null;
            while (bitmapImage is null)
            {
#pragma warning disable 0612
                bitmapImage = await ImageCacheHelper.GetImageAsyncOld(Type, Uri, this, inAppNotify);
#pragma warning restore 0612
            }
            await Task.Delay(20);
            Pic = bitmapImage;
        }

        private static ImageType ChangeType(ImageType type)
        {
            switch (type)
            {
                case ImageType.SmallImage:
                    return ImageType.OriginImage;

                case ImageType.SmallAvatar:
                    return ImageType.BigAvatar;

                default:
                    return type;
            }
        }

        public void ChangeType()
        {
            Type = ChangeType(Type);
            GetImage();
        }
    }

    public sealed partial class ShowImagePage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null)
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private readonly ObservableCollection<ImageModel> imageModels = new ObservableCollection<ImageModel>();
        private bool isBackButtonEnabled;
        private bool isForwardButtonEnabled;
        private bool isOriginButtonEnabled;
        private int selectedIndex;

        private bool IsBackButtonEnabled
        {
            get => isBackButtonEnabled;
            set
            {
                isBackButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool IsForwardButtonEnabled
        {
            get => isForwardButtonEnabled;
            set
            {
                isForwardButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        private bool IsOriginButtonEnabled
        {
            get => isOriginButtonEnabled;
            set
            {
                isOriginButtonEnabled = value;
                RaisePropertyChangedEvent();
            }
        }

        public int SelectedIndex
        {
            get => selectedIndex;
            set
            {
                selectedIndex = value;
                RaisePropertyChangedEvent();
            }
        }

        public ShowImagePage()
        {
            InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            Windows.UI.ViewManagement.ApplicationViewTitleBar titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            titleBar.InactiveBackgroundColor = titleBar.ButtonInactiveBackgroundColor = titleBar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            Window.Current.SetTitleBar(titleBorder);
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Models.ImageModel model)
            {
                if (model.ContextArray.IsDefaultOrEmpty)
                {
                    imageModels.Add(new ImageModel(model, notify, Dispatcher));
                }
                else
                {
                    foreach (Models.ImageModel item in model.ContextArray)
                    {
                        imageModels.Add(new ImageModel(item, notify, Dispatcher));
                    }
                    _ = Task.Run(async () =>
                      {
                          await Task.Delay(20);
                          await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => SFlipView.SelectedIndex = model.ContextArray.IndexOf(model));
                      });
                }
            }
            else if (e.Parameter is object[] param)
            {
                imageModels.Add(new ImageModel((string)param[0], (ImageType)param[1], notify, Dispatcher));
            }
        }

        private static void ScrollViewerMain_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            ScrollViewer view = sender as ScrollViewer;
            if (view.ZoomFactor != 2)
            {
                _ = view.ChangeView(view.HorizontalOffset * 2, view.VerticalOffset * 2, 2);
            }
            else
            {
                _ = view.ChangeView(view.HorizontalOffset / view.ZoomFactor, view.VerticalOffset / view.ZoomFactor, 1);
            }
        }

        private static void ScrollViewerMain_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ScrollViewer view = (sender as FrameworkElement).Parent as ScrollViewer;
            _ = view.ChangeView(view.HorizontalOffset - e.Delta.Translation.X * view.ZoomFactor, view.VerticalOffset - e.Delta.Translation.Y * view.ZoomFactor, null);
        }

        private void ShowMenuFlyout()
        {
            SFlipView.ContextFlyout.ShowAt(SFlipView);
        }

        private void SFlipView_RightTapped(object sender, RightTappedRoutedEventArgs e) => ShowMenuFlyout();

        private void Page_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Escape:
                    Window.Current.Close();
                    goto end;
                case Windows.System.VirtualKey.Menu:
                    ShowMenuFlyout();
                    goto end;
                end:
                    e.Handled = true;
                    break;
            }
        }

        private void ResetDegree(int index)
        {
            Storyboard storyboard = null;
            System.Collections.Generic.IEnumerable<Image> images = SFlipView.FindDescendants().OfType<Image>();
            foreach (Image item in images)
            {
                if (item.DataContext != imageModels[index]) { continue; }

                storyboard = (Storyboard)item.Resources["storyboard"];
                break;
            }

            if (storyboard == null) { return; }

            DoubleAnimation a = (DoubleAnimation)storyboard.Children[0];
            if (a.From == -90) { return; }

            a.To = 0;
            storyboard.Begin();
            a.From = -90;
        }

        private void SFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageModels.Count == 0 || SFlipView.SelectedIndex == -1) { return; }

            int i = SFlipView.SelectedIndex + 1;
            SelectedIndex = i;
            if (i == 1)
            {
                IsBackButtonEnabled = false;
                IsForwardButtonEnabled = imageModels.Count != i;
            }
            else if (i >= imageModels.Count)
            {
                IsBackButtonEnabled = true;
                IsForwardButtonEnabled = false;
            }
            else
            {
                IsBackButtonEnabled = IsForwardButtonEnabled = true;
            }

            if (SFlipView.SelectedIndex - 1 > -1)
            {
                ResetDegree(SFlipView.SelectedIndex - 1);
            }
            if (SFlipView.SelectedIndex + 1 < imageModels.Count)
            {
                ResetDegree(SFlipView.SelectedIndex + 1);
            }

            switch (imageModels[SFlipView.SelectedIndex].Type)
            {
                case ImageType.BigAvatar:
                case ImageType.OriginImage:
                    IsOriginButtonEnabled = false;
                    break;

                default:
                    IsOriginButtonEnabled = true;
                    break;
            }
        }

        private void OperateWithScrollViewers(Action<ScrollViewer> action)
        {
            System.Collections.Generic.IEnumerable<ScrollViewer> views = SFlipView.FindDescendants().OfType<ScrollViewer>();
            int n = 0;
            foreach (ScrollViewer item in views)
            {
                if (n != 0 && n == selectedIndex)
                {
                    action(item);
                    break;
                }
                n++;
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (((FrameworkElement)sender).Tag as string)
            {
                case "back":
                    if (SFlipView.SelectedIndex > 0)
                    {
                        SFlipView.SelectedIndex -= 1;
                    }
                    else
                    {
                        IsBackButtonEnabled = false;
                        IsForwardButtonEnabled = imageModels.Count != 1;
                    }
                    break;

                case "forward":
                    if (SFlipView.SelectedIndex < imageModels.Count - 1)
                    {
                        SFlipView.SelectedIndex += 1;
                    }
                    else
                    {
                        IsBackButtonEnabled = true;
                        IsForwardButtonEnabled = false;
                    }
                    break;

                case "zoomIn":
                    OperateWithScrollViewers((item) =>
                    {
                        float factor = item.ZoomFactor;
                        if (factor + 0.25 < 3)
                        {
                            _ = item.ChangeView(item.HorizontalOffset / factor * (factor + 0.25), item.VerticalOffset / factor * (factor + 0.25), factor + 0.25F);
                        }
                    });
                    break;

                case "zoomOut":
                    OperateWithScrollViewers((item) =>
                    {
                        float factor = item.ZoomFactor;
                        if (factor - 0.25 > 0.5)
                        {
                            _ = item.ChangeView(item.HorizontalOffset / factor * (factor - 0.25), item.VerticalOffset / factor * (factor - 0.25), factor - 0.25F);
                        }
                    });
                    break;

                case "rotate":
                    if (SFlipView.SelectedIndex == -1 || SFlipView.Items.Count == 0) { return; }

                    Storyboard storyboard = null;
                    System.Collections.Generic.IEnumerable<Image> images = SFlipView.FindDescendants().OfType<Image>();
                    foreach (Image item in images)
                    {
                        if (item.DataContext != imageModels[SFlipView.SelectedIndex]) { continue; }

                        storyboard = (Storyboard)item.Resources["storyboard"];
                        break;
                    }

                    if (storyboard == null) { return; }

                    DoubleAnimation a = (DoubleAnimation)storyboard.Children[0];
                    a.From = a.To;
                    a.To += 90;
                    storyboard.Begin();
                    break;

                case "origin":
                    imageModels[SFlipView.SelectedIndex].ChangeType();
                    IsOriginButtonEnabled = false;
                    break;

                case "save":
                    string u = imageModels[SFlipView.SelectedIndex].Uri;
                    string fileName = u.Substring(u.LastIndexOf('/') + 1);
                    FileSavePicker fileSavePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty, StringComparison.Ordinal)
                    };

                    string uu = fileName.Substring(fileName.LastIndexOf('.') + 1);
                    int index = uu.IndexOfAny(new char[] { '?', '%', '&' });
                    uu = uu.Substring(0, index == -1 ? uu.Length : index);
                    fileSavePicker.FileTypeChoices.Add($"{uu}文件", new string[] { "." + uu });

                    StorageFile file = await fileSavePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        using (Stream fs = await file.OpenStreamForWriteAsync())
                        {
                            StorageFolder folder = await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(imageModels[SFlipView.SelectedIndex].Type.ToString());
                            StorageFile storageFile = await folder.GetFileAsync(Core.Helpers.Utils.GetMD5(u));
                            using (Stream s = (await storageFile.OpenReadAsync()).AsStreamForRead())
                            {
                                await s.CopyToAsync(fs);
                            }
                        }
                    }
                    break;

                case "uri":
                    DataPackage data = new DataPackage();
                    data.SetText(imageModels[SFlipView.SelectedIndex].Uri);
                    Clipboard.SetContent(data);
                    break;
            }
        }
    }
}