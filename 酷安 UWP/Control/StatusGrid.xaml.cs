using CoolapkUWP.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace CoolapkUWP.Control
{
    public sealed partial class StatusGrid : UserControl
    {
        public StatusGrid()
        {
            InitializeComponent();
            Width = Window.Current.Bounds.Width + 140;
            Window.Current.SizeChanged += (s, e) => Width = e.Size.Width + 140;
            if (UIHelper.isShowingProgressBar) { ShowProgressBar(); }
            else { HideProgressBar(); }
            Rectangle_PointerExited();
        }

        public void ShowProgressBar()
        {
            statusBar.Visibility = Visibility.Visible;
            statusBar.IsIndeterminate = true;
            statusBar.ShowError = false;
            statusBar.ShowPaused = false;
        }

        public void PausedProgressBar()
        {
            statusBar.Visibility = Visibility.Visible;
            statusBar.IsIndeterminate = true;
            statusBar.ShowError = false;
            statusBar.ShowPaused = true;
        }

        public void ErrorProgressBar()
        {
            statusBar.Visibility = Visibility.Visible;
            statusBar.IsIndeterminate = true;
            statusBar.ShowPaused = false;
            statusBar.ShowError = true;
        }

        public void HideProgressBar()
        {
            statusBar.Visibility = Visibility.Collapsed;
            statusBar.IsIndeterminate = false;
            statusBar.ShowError = false;
            statusBar.ShowPaused = false;
        }

        public void ShowMessage(string message)
        {
            messageTextBlock.Text = message;
            Rectangle_PointerEntered();
        }

        public void Rectangle_PointerEntered()
        {
            EnterStoryboard.Begin();
        }

        public void Rectangle_PointerExited()
        {
            ExitStoryboard.Begin();
        }
    }
}
