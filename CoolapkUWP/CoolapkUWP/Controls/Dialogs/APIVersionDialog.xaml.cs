using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls.Dialogs
{
    public sealed partial class APIVersionDialog : ContentDialog
    {
        internal APIVersion APIVersion { get; set; }

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
    }
}
