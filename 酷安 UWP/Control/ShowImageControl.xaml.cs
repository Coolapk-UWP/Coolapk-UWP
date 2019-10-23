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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace 酷安_UWP
{
    public sealed partial class ShowImageControl : UserControl
    {
        bool _Hide;
        public bool Hide
        {
            get => _Hide;
            set
            {
                Visibility = value ? Visibility.Collapsed : Visibility.Visible;
                _Hide = value;
            }
        }

        public ShowImageControl()
        {
            this.InitializeComponent();
            Hide = true;
        }

        public void ShowImage(string url)
        {
            Hide = false;
            SFlipView.ItemsSource = new ImageSource[1] { new BitmapImage(new Uri(url)) };
        }

        private void CloseFlip_Click(object sender, RoutedEventArgs e) => Hide = true;

        private void UserControl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Escape)
            {
                e.Handled = true;
                Hide = true;
            }
        }
    }
}
