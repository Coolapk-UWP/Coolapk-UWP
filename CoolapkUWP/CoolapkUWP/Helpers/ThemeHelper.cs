using CoolapkUWP.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;

namespace CoolapkUWP.Helpers
{
    /// <summary>
    /// Class providing functionality around switching and restoring theme settings
    /// </summary>
    public static class ThemeHelper
    {
        private static Window CurrentApplicationWindow;

        // Keep reference so it does not get optimized/garbage collected
        public static UISettings UISettings;

        public static WeakEvent<UISettingChangedType> UISettingChanged { get; } = new WeakEvent<UISettingChangedType>();

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                if (CurrentApplicationWindow?.Content is FrameworkElement rootElement)
                {
                    if (rootElement.RequestedTheme != ElementTheme.Default)
                    {
                        return rootElement.RequestedTheme;
                    }
                }

                return SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                return CurrentApplicationWindow?.Content is FrameworkElement rootElement ? rootElement.RequestedTheme : ElementTheme.Default;
            }
            set
            {
                if (CurrentApplicationWindow?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = value;
                }

                SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
                UpdateSystemCaptionButtonColors();
                UISettingChanged.Invoke(IsDarkTheme() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode);
            }
        }

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = Window.Current ?? App.MainWindow;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UiSettings_ColorValuesChanged;
        }

        private static async void UiSettings_ColorValuesChanged(UISettings sender, object args)
        {
            // Make sure we have a reference to our window so we dispatch a UI change
            if (CurrentApplicationWindow != null)
            {
                // Dispatch on UI thread so that we have a current appbar to access and change
                _ = CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(UpdateSystemCaptionButtonColors, CoreDispatcherPriority.High);
                UISettingChangedType type = await CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(()=>IsDarkTheme() ? UISettingChangedType.DarkMode : UISettingChangedType.LightMode, CoreDispatcherPriority.High);
                UISettingChanged.Invoke(type);
            }
        }

        public static bool IsDarkTheme()
        {
            return RootTheme == ElementTheme.Default
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                : RootTheme == ElementTheme.Dark;
        }

        public static async Task<bool> IsDarkThemeAsync()
        {
            // Make sure we have a reference to our window so we dispatch a UI change
            if (CurrentApplicationWindow != null)
            {
                // Dispatch on UI thread so that we have a current appbar to access and change
                return await CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(
                    () => RootTheme == ElementTheme.Default
                        ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                        : RootTheme == ElementTheme.Dark,
                    CoreDispatcherPriority.High);
            }
            return RootTheme == ElementTheme.Default
                ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                : RootTheme == ElementTheme.Dark;
        }

        public static void UpdateSystemCaptionButtonColors()
        {
            bool IsDark = IsDarkTheme();
            bool IsHighContrast = new AccessibilitySettings().HighContrast;

            Color ForegroundColor = IsDark || IsHighContrast ? Colors.White : Colors.Black;
            Color BackgroundColor = IsHighContrast ? Color.FromArgb(255, 0, 0, 0) : IsDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            if (UIHelper.HasStatusBar)
            {
                StatusBar StatusBar = StatusBar.GetForCurrentView();
                StatusBar.ForegroundColor = ForegroundColor;
                StatusBar.BackgroundColor = BackgroundColor;
                StatusBar.BackgroundOpacity = 0; // 透明度
            }
            else
            {
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                TitleBar.ForegroundColor = TitleBar.ButtonForegroundColor = ForegroundColor;
                TitleBar.BackgroundColor = TitleBar.InactiveBackgroundColor = BackgroundColor;
                TitleBar.ButtonBackgroundColor = TitleBar.ButtonInactiveBackgroundColor = UIHelper.HasTitleBar ? BackgroundColor : Colors.Transparent;
            }
        }
    }

    public enum UISettingChangedType
    {
        LightMode,
        DarkMode,
        NoPicChanged,
    }
}
