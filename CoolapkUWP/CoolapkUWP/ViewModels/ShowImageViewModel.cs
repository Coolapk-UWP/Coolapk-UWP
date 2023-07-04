using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace CoolapkUWP.ViewModels
{
    public class ShowImageViewModel : IViewModel
    {
        private string ImageName = string.Empty;

        public CoreDispatcher Dispatcher { get; }

        private string title;
        public string Title
        {
            get => title;
            protected set
            {
                if (title != value)
                {
                    title = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private int index = -1;
        public int Index
        {
            get => index;
            set
            {
                if (index != value)
                {
                    if (index != -1) { ResigerImage(Images[index], Images[value]); }
                    index = value;
                    RaisePropertyChangedEvent();
                    Title = GetTitle(Images[value].Uri);
                    ShowOrigin = Images[value].Type.HasFlag(ImageType.Small);
                }
            }
        }

        private bool isLoading;
        public bool IsLoading
        {
            get => isLoading;
            protected set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool isShowHub = true;
        public bool IsShowHub
        {
            get => isShowHub;
            set
            {
                if (isShowHub != value)
                {
                    isShowHub = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private IList<ImageModel> images;
        public IList<ImageModel> Images
        {
            get => images;
            private set
            {
                if (images != value)
                {
                    images = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        private bool showOrigin = false;
        public bool ShowOrigin
        {
            get => showOrigin;
            set
            {
                if (showOrigin != value)
                {
                    showOrigin = value;
                    RaisePropertyChangedEvent();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                if (Dispatcher?.HasThreadAccess == false)
                {
                    await Dispatcher.ResumeForegroundAsync();
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public ShowImageViewModel(ImageModel image, CoreDispatcher dispatcher)
        {
            Dispatcher = dispatcher;
            if (image.ContextArray.Any())
            {
                Images = image.ContextArray;
                Index = Images.IndexOf(image);
            }
            else
            {
                Images = new List<ImageModel> { image };
                Index = 0;
            }
            if (SettingsHelper.Get<bool>(SettingsHelper.IsDisplayOriginPicture))
            {
                foreach (ImageModel Image in Images)
                {
                    Image.Type &= (ImageType)0xFE;
                }
            }
        }

        ~ShowImageViewModel()
        {
            foreach (ImageModel image in images)
            {
                image.LoadStarted -= OnLoadStarted;
                image.LoadCompleted -= OnLoadCompleted;
            }
        }

        public async Task Refresh(bool reset = false) => await Images[Index].Refresh();

        bool IViewModel.IsEqual(IViewModel other) => other is ShowImageViewModel model && IsEqual(model);

        public bool IsEqual(ShowImageViewModel other) => Images == other.Images;

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            ImageName = regex.IsMatch(url) ? regex.Match(url).Value : "查看图片";
            return $"{ImageName} ({Index + 1}/{Images.Count})";
        }

        public async void CopyPic()
        {
            DataPackage dataPackage = await GetImageDataPackage("复制图片");
            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic()
        {
            DataPackage dataPackage = await GetImageDataPackage("分享图片");
            if (dataPackage != null)
            {
                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
                dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
                DataTransferManager.ShowShareUI();
            }
        }

        public async Task<DataPackage> GetImageDataPackage(string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return null;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = ImageName;

            return dataPackage;
        }

        public async Task GetImageDataPackage(DataPackage dataPackage, string title)
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = title;
            dataPackage.Properties.Description = ImageName;
            dataPackage.SetStorageItems(new IStorageItem[] { file });
        }

        public async void SavePic()
        {
            string url = Images[Index].Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
                return;
            }

            string fileName = ImageName;
            FileSavePicker fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = fileName.Replace(fileName.Substring(fileName.LastIndexOf('.')), string.Empty)
            };

            string fileex = fileName.Substring(fileName.LastIndexOf('.') + 1);
            int index = fileex.IndexOfAny(new char[] { '?', '%', '&' });
            fileex = fileex.Substring(0, index == -1 ? fileex.Length : index);
            fileSavePicker.FileTypeChoices.Add($"{fileex}文件", new string[] { "." + fileex });

            StorageFile file = await fileSavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (Stream FolderStream = await file.OpenStreamForWriteAsync())
                {
                    using (IRandomAccessStreamWithContentType RandomAccessStream = await image.OpenReadAsync())
                    {
                        using (Stream ImageStream = RandomAccessStream.AsStreamForRead())
                        {
                            await ImageStream.CopyToAsync(FolderStream);
                        }
                    }
                }
            }
        }

        private void ResigerImage(ImageModel oldvalue, ImageModel newvalue)
        {
            oldvalue.LoadStarted -= OnLoadStarted;
            oldvalue.LoadCompleted -= OnLoadCompleted;
            newvalue.LoadStarted += OnLoadStarted;
            newvalue.LoadCompleted += OnLoadCompleted;
        }

        private void OnLoadStarted(ImageModel sender, object args) => IsLoading = true;

        private void OnLoadCompleted(ImageModel sender, object args) => IsLoading = false;
    }
}
