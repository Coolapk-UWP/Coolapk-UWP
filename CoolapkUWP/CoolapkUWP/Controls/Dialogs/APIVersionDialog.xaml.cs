using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using CoolapkUWP.Helpers.Converters;
using CoolapkUWP.Models.Update;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls.Dialogs
{
    public sealed partial class APIVersionDialog : ContentDialog, INotifyPropertyChanged
    {
        internal APIVersion APIVersion { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void RaisePropertyChangedEvent([CallerMemberName] string name = null)
        {
            if (name != null)
            {
                await Dispatcher.ResumeForegroundAsync();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        public APIVersionDialog(string line)
        {
            InitializeComponent();
            APIVersion = APIVersion.Parse(line);
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                SettingsHelper.Set(SettingsHelper.CustomAPI, APIVersion);
                NetworkHelper.SetRequestHeaders();
            }
        }

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            UIHelper.ShowProgressBar();
            if (await APIVersion.GetLatestAsync() is APIVersion version)
            {
                APIVersion = version;
            }
            UIHelper.HideProgressBar();
        }
    }

    public class Int32ToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string result = value?.ToString();
            return ConverterTools.Convert(result, targetType);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            int result;
            switch (value)
            {
                case null:
                    result = 0;
                    break;
                case string str:
                    int.TryParse(str, out result);
                    break;
                default:
                    result = System.Convert.ToInt32(value);
                    break;
            }
            return ConverterTools.Convert(result, targetType);
        }
    }
}
