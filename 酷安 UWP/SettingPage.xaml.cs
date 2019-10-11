using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace 酷安_UWP
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingPage : Page
    {
        public SettingPage()
        {
            this.InitializeComponent();
        }

        ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
        bool IsLight//标题栏颜色
        {
            set
            {
                if (value == true)
                    TitleBar.ButtonInactiveBackgroundColor =
                        TitleBar.ButtonBackgroundColor =
                        TitleBar.InactiveBackgroundColor =
                        TitleBar.BackgroundColor = Windows.UI.Color.FromArgb(255, 252, 252, 255);
                else
                    TitleBar.ButtonInactiveBackgroundColor =
                        TitleBar.ButtonBackgroundColor =
                        TitleBar.InactiveBackgroundColor =
                        TitleBar.BackgroundColor = Windows.UI.Color.FromArgb(255, 18, 18, 21);
            }
        }

        //Delegate
        public delegate void ThemeChangeHandler(object sender, ElementTheme Theme);
        public event ThemeChangeHandler ThemeChange = null;


        private void CleanDataButton_Click(System.Object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values.Clear();
            var cookieManager = new Windows.Web.Http.Filters.HttpBaseProtocolFilter().CookieManager;
            foreach (var item in cookieManager.GetCookies(new Uri("http://account.coolapk.com")))
                cookieManager.DeleteCookie(item);
        }

        private void ChangeThemeButton_Click(System.Object sender, RoutedEventArgs e)
        {
            if (Window.Current.Content is FrameworkElement frameworkElement)
            {
                if (frameworkElement.RequestedTheme == ElementTheme.Dark)
                {
                    frameworkElement.RequestedTheme = ElementTheme.Light;
                    //ToLight.Begin();//Storyboard
                    IsLight = true;//标题栏颜色
                    ThemeChange?.Invoke(this, ElementTheme.Light);//Delegate
                }
                else
                {
                    frameworkElement.RequestedTheme = ElementTheme.Dark;
                    //ToNight.Begin();//Storyboard
                    IsLight = false;//标题栏颜色
                    ThemeChange?.Invoke(this, ElementTheme.Dark);//Delegate
                }
            }
        }

        private void ToggleSwitch_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
    }
}
