using CoolapkUWP.Helpers;
using CoolapkUWP.Models.Update;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class UserAgentDialog : ContentDialog
    {
        internal UserAgent UserAgent { get; set; }

        public UserAgentDialog(string line)
        {
            InitializeComponent();
            UserAgent = UserAgent.Parse(line);
            Closing += OnClosing;
        }

        private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary)
            {
                SettingsHelper.Set(SettingsHelper.CustomUA, UserAgent);
                NetworkHelper.SetRequestHeaders();
            }
        }
    }
}
