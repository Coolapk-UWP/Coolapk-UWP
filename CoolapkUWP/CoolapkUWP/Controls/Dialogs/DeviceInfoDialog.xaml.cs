using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using System;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls.Dialogs
{
    public sealed partial class DeviceInfoDialog : ContentDialog
    {
        #region DeviceInfo

        /// <summary>
        /// Identifies the <see cref="DeviceInfo"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DeviceInfoProperty =
            DependencyProperty.Register(
                nameof(DeviceInfo),
                typeof(DeviceInfo),
                typeof(DeviceInfoDialog),
                null);

        public DeviceInfo DeviceInfo
        {
            get => (DeviceInfo)GetValue(DeviceInfoProperty);
            private set => SetValue(DeviceInfoProperty, value);
        }

        #endregion

        public DeviceInfoDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            DeviceInfo = SettingsHelper.Get<DeviceInfo>(SettingsHelper.DeviceInfo);
            Clipboard.ContentChanged += Clipboard_ContentChanged;
            Clipboard_ContentChanged(null, null);
        }

        private void ContentDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            Clipboard.ContentChanged -= Clipboard_ContentChanged;
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                NetworkHelper.UpdateDeviceInfo(DeviceInfo);
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            switch ((sender as FrameworkElement).Tag.ToString())
            {
                case "Copy":
                    DataPackage dataPackage = new DataPackage();
                    dataPackage.SetText(DeviceInfo.CreateDeviceCode());
                    Clipboard.SetContent(dataPackage);
                    break;
                case "Paste":
                    DataPackageView dataPackageView = Clipboard.GetContent();
                    if (dataPackageView.Contains(StandardDataFormats.Text))
                    {
                        string text = await dataPackageView.GetTextAsync();
                        if (DeviceInfo.CreateByDeviceCode(text) is DeviceInfo deviceInfo)
                        {
                            DeviceInfo = deviceInfo;
                        }
                    }
                    break;
                case "Reset":
                    DeviceInfo = DeviceInfo.Default;
                    break;
            }
        }

        private async void Clipboard_ContentChanged(object sender, object e)
        {
            try
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = await dataPackageView.GetTextAsync();
                    text = text.Reverse();
                    int index = text.Length % 4;
                    byte[] bytes = Convert.FromBase64String(index == 0 ? text : $"{text}{new string(Enumerable.Repeat('=', 4 - index).ToArray())}");
                    string result = Encoding.UTF8.GetString(bytes);
                    await Dispatcher.ResumeForegroundAsync();
                    PasteItem.IsEnabled = result.Split(';').Length >= 8;
                }
            }
            catch
            {
                await Dispatcher.ResumeForegroundAsync();
                PasteItem.IsEnabled = false;
            }
        }
    }
}
