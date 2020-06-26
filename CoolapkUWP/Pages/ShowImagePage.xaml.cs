using CoolapkUWP.Helpers;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
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
        public bool IsGif { get => Uri.Substring(Uri.LastIndexOf('.')).ToLower().Contains("gif", StringComparison.Ordinal); }

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
            if (name != null)
            {
                _ = dispatcher?.RunAsync(CoreDispatcherPriority.Normal, () =>
                   {
                       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
                   });
            }
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
            var bitmapImage = await ImageCacheHelper.GetImageAsync(Type, Uri, this, inAppNotify);
            await Task.Delay(20);
            Pic = bitmapImage;
        }

        private ImageType ChangeType(ImageType type)
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
            this.InitializeComponent();
            Windows.ApplicationModel.Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
            var bar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
            bar.InactiveBackgroundColor = bar.ButtonInactiveBackgroundColor = bar.ButtonBackgroundColor = Windows.UI.Colors.Transparent;
            Window.Current.SetTitleBar(titleBorder);
        }

        protected override void OnNavigatedTo(Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Models.ImageModel model)
            {
                if (model.ContextArray.Length == 0)
                {
                    imageModels.Add(new ImageModel(model, notify, Dispatcher));
                }
                else
                {
                    foreach (var item in model.ContextArray)
                    {
                        imageModels.Add(new ImageModel(item, notify, Dispatcher));
                    }
                    Task.Run(async () =>
                    {
                        await Task.Delay(20);
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => SFlipView.SelectedIndex = model.ContextArray.IndexOf(model));
                    });
                }
            }
            else if (e.Parameter is object[] param)
            {
                imageModels.Add(new ImageModel((string)param[0], (ImageType)param[1], notify, Dispatcher));
            }
        }

        private void ScrollViewerMain_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var view = sender as ScrollViewer;
            if (view.ZoomFactor != 2)
            {
                view.ChangeView(view.HorizontalOffset * 2, view.VerticalOffset * 2, 2);
            }
            else
            {
                view.ChangeView(view.HorizontalOffset / view.ZoomFactor, view.VerticalOffset / view.ZoomFactor, 1);
            }
        }

        private void ScrollViewerMain_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var view = (sender as FrameworkElement).Parent as ScrollViewer;
            view.ChangeView(view.HorizontalOffset - e.Delta.Translation.X * view.ZoomFactor, view.VerticalOffset - e.Delta.Translation.Y * view.ZoomFactor, null);
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

        private void SFlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (imageModels.Count == 0 || SFlipView.SelectedIndex == -1) { return; }

            var i = SFlipView.SelectedIndex + 1;
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
                    var views = Microsoft.Toolkit.Uwp.UI.Extensions.VisualTree.FindDescendants<ScrollViewer>(SFlipView);
                    int n1 = 0;
                    foreach (var item in views)
                    {
                        if (n1 != 0 && n1 == selectedIndex)
                        {
                            var factor = item.ZoomFactor;
                            if (factor + 0.25 < 3)
                            {
                                item.ChangeView(item.HorizontalOffset / factor * (factor + 0.25), item.VerticalOffset / factor * (factor + 0.25), factor + 0.25F);
                            }
                            break;
                        }
                        n1++;
                    }
                    break;

                case "zoomOut":
                    var viewers = Microsoft.Toolkit.Uwp.UI.Extensions.VisualTree.FindDescendants<ScrollViewer>(SFlipView);
                    int n2 = 0;
                    foreach (var item in viewers)
                    {
                        if (n2 != 0 && n2 == selectedIndex)
                        {
                            var factor = item.ZoomFactor;
                            if (factor - 0.25 > 0.5)
                            {
                                item.ChangeView(item.HorizontalOffset / factor * (factor - 0.25), item.VerticalOffset / factor * (factor - 0.25), factor - 0.25F);
                            }
                            break;
                        }
                        n2++;
                    }
                    break;

                case "origin":
                    imageModels[SFlipView.SelectedIndex].ChangeType();
                    IsOriginButtonEnabled = false;
                    break;

                case "save":
                    string u = imageModels[SFlipView.SelectedIndex].Uri;
                    string fileName = u.Substring(u.LastIndexOf('/') + 1);
                    var fileSavePicker = new FileSavePicker
                    {
                        SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                        SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty, StringComparison.Ordinal)
                    };

                    string uu = fileName.Substring(fileName.LastIndexOf('.') + 1);
                    int index = uu.IndexOfAny(new char[] { '?', '%', '&' });
                    uu = uu.Substring(0, index == -1 ? uu.Length : index);
                    fileSavePicker.FileTypeChoices.Add($"{uu}文件", new string[] { "." + uu });

                    var file = await fileSavePicker.PickSaveFileAsync();
                    if (file != null)
                    {
                        using (Stream fs = await file.OpenStreamForWriteAsync())
                        using (Stream s = (await (await (await ApplicationData.Current.LocalCacheFolder.GetFolderAsync(imageModels[SFlipView.SelectedIndex].Type.ToString())).GetFileAsync(DataHelper.GetMD5(u))).OpenReadAsync()).AsStreamForRead())
                        {
                            await s.CopyToAsync(fs);
                        }
                    }
                    break;
            }
        }
    }
}