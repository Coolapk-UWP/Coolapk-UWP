using CoolapkUWP.Controls;
using CoolapkUWP.Helpers;
using CoolapkUWP.ViewModels.BrowserPages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace CoolapkUWP.Pages.SettingsPages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class TestPage : Page
    {
        public TestPage()
        {
            InitializeComponent();
            Test();
        }

        private void Test()
        {
            var Picker = new Picker
            {
                Content = new TextBlock
                {
                    Text = "Hello"
                },
                PopupTransitions = new TransitionCollection
                {
                    new EdgeUIThemeTransition
                    {
                        Edge = EdgeTransitionLocation.Bottom
                    }
                }
            };
            Picker.Show();
        }
    }
}
