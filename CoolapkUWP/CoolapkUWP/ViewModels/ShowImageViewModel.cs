using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Images;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;

namespace CoolapkUWP.ViewModels
{
    public class ShowImageViewModel : INotifyPropertyChanged, IViewModel
    {
        private ImageModel BaseImage;
        private string ImageName = string.Empty;
        public double[] VerticalOffsets { get; set; } = new double[1];

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

        private int index;
        public int Index
        {
            get => index;
            set
            {
                ResigerImage(Images[index], Images[value]);
                index = value;
                RaisePropertyChangedEvent();
                Title = GetTitle(Images[value].Uri);
                ShowOrigin = Images[value].Type == ImageType.SmallImage || Images[value].Type == ImageType.SmallAvatar;
            }
        }

        private bool isLoading = true;
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

        private void RaisePropertyChangedEvent([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        {
            if (name != null) { PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }

        public ShowImageViewModel(ImageModel image) => BaseImage = image;

        public void Initialize()
        {
            Images = BaseImage.ContextArray.Any()
                ? BaseImage.ContextArray.Select(x => new ImageModel(x.Uri, ImageType.SmallImage)).ToList()
                : (IList<ImageModel>)new List<ImageModel> { new ImageModel(BaseImage.Uri, ImageType.SmallImage) };
            Index = BaseImage.ContextArray.Any() ? BaseImage.ContextArray.IndexOf(BaseImage) : 0;
        }

        public async Task Refresh(bool reset = false) => await Images[Index].Refresh();

        private string GetTitle(string url)
        {
            Regex regex = new Regex(@"[^/]+(?!.*/)");
            ImageName = regex.IsMatch(url) ? regex.Match(url).Value : "查看图片";
            return $"{ImageName} ({Index + 1}/{Images.Count})";
        }

        public async void CopyPic()
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = "分享图片";
            dataPackage.Properties.Description = ImageName;

            Clipboard.SetContentWithOptions(dataPackage, null);
        }

        public async void SharePic()
        {
            StorageFile file = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, Images[Index].Uri);
            if (file == null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
            }
            RandomAccessStreamReference bitmap = RandomAccessStreamReference.CreateFromFile(file);

            DataPackage dataPackage = new DataPackage();
            dataPackage.SetBitmap(bitmap);
            dataPackage.Properties.Title = "分享图片";
            dataPackage.Properties.Description = ImageName;

            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested += (sender, args) => { args.Request.Data = dataPackage; };
            DataTransferManager.ShowShareUI();
        }

        public async void SavePic()
        {
            string url = Images[Index].Uri;
            StorageFile image = await ImageCacheHelper.GetImageFileAsync(ImageType.OriginImage, url);
            if (image != null)
            {
                string str = ResourceLoader.GetForViewIndependentUse().GetString("ImageLoadError");
                UIHelper.ShowMessage(str);
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
