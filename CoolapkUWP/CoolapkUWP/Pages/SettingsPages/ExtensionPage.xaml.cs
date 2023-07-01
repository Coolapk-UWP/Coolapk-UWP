using CoolapkUWP.Common;
using CoolapkUWP.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class ExtensionPage : Page
    {
        internal ExtensionManager Provider;

        public string Title => ResourceLoader.GetForCurrentView("MainPage").GetString("Extension");

        public ExtensionPage() => InitializeComponent();

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Provider = Provider ?? new ExtensionManager(ExtensionManager.OSSUploader);
            DataContext = Provider;
            await Refresh(true);
        }

        public async Task Refresh(bool reset = false)
        {
            UIHelper.ShowProgressBar();
            if (reset)
            {
                await Provider.Initialize(Dispatcher);
            }
            else
            {
                await Provider.FindAndLoadExtensions();
            }
            UIHelper.HideProgressBar();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            switch (element.Name)
            {
                case "Uninstall":
                    Provider.RemoveExtension(element.Tag as Extension);
                    break;
                default:
                    break;
            }
        }

    }
}
