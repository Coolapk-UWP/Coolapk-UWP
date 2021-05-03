using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
            //if (Tools.isShowingProgressBar) ShowProgressBar();
            //else HideProgressBar();
            Rectangle_PointerExited();
        }

        public void ShowProgressBar()
        {
            statusBar.Visibility = Visibility.Visible;
            statusBar.IsIndeterminate = true;
        }

        public void HideProgressBar()
        {
            if (!string.IsNullOrEmpty(messageTextBlock.Text)) return;
            statusBar.Visibility = Visibility.Collapsed;
            statusBar.IsIndeterminate = false;
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
