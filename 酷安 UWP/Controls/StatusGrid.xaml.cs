using CoolapkUWP.Helpers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Controls
{
    public sealed partial class StatusGrid : UserControl
    {
        public StatusGrid()
        {
            this.InitializeComponent();
            Width = Window.Current.Bounds.Width;
            Window.Current.SizeChanged += (s, e) => Width = e.Size.Width;
            if (UIHelper.isShowingProgressBar) ShowProgressBar();
            else HideProgressBar();
        }

        public void ShowProgressBar()
        {
            statusBar.Visibility = Visibility.Visible;
        }

        public void HideProgressBar()
        {
            if (!string.IsNullOrEmpty(messageTextBlock.Text)) return;
            statusBar.Visibility = Visibility.Collapsed;
        }

        public void ShowMessage(string message) => messageTextBlock.Text = message;
    }
}
