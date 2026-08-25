using CoolapkUWP.Common;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.WindowManagement;
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

        #region UISettingChanged

        private static readonly WeakEvent<ApplicationTheme> actions = new WeakEvent<ApplicationTheme>();

        public static event Action<ApplicationTheme> UISettingChanged
        {
            add => actions.Add(value);
            remove => actions.Remove(value);
        }

        public static void InvokeUISettingChanged(ApplicationTheme value) => actions.Invoke(value);

        #endregion

        #region NoPicsModeChanged

        private static readonly WeakEvent<bool> nopic = new WeakEvent<bool>();

        public static event Action<bool> NoPicsModeChanged
        {
            add => nopic.Add(value);
            remove => nopic.Remove(value);
        }

        public static void InvokeNoPicsModeChanged(bool value) => nopic.Invoke(value);

        #endregion

        /// <summary>
        /// Gets the current actual theme of the app based on the requested theme of the
        /// root element, or if that value is Default, the requested theme of the Application.
        /// </summary>
        public static ElementTheme ActualTheme
        {
            get
            {
                return CurrentApplicationWindow == null
                    ? SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                    : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                        ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                            && rootElement.RequestedTheme != ElementTheme.Default
                                ? rootElement.RequestedTheme
                                : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme)
                        : UIHelper.AwaitByTaskCompleteSource(() =>
                            CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                                CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                    && _rootElement.RequestedTheme != ElementTheme.Default
                                        ? _rootElement.RequestedTheme
                                        : SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme),
                                CoreDispatcherPriority.High));
            }
        }

        /// <summary>
        /// Gets or sets (with LocalSettings persistence) the RequestedTheme of the root element.
        /// </summary>
        public static ElementTheme RootTheme
        {
            get
            {
                return CurrentApplicationWindow == null
                    ? ElementTheme.Default
                    : CurrentApplicationWindow.Dispatcher.HasThreadAccess
                        ? CurrentApplicationWindow.Content is FrameworkElement rootElement
                            ? rootElement.RequestedTheme
                            : ElementTheme.Default
                        : UIHelper.AwaitByTaskCompleteSource(() =>
                            CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                                CurrentApplicationWindow.Content is FrameworkElement _rootElement
                                    ? _rootElement.RequestedTheme
                                    : ElementTheme.Default,
                                CoreDispatcherPriority.High));
            }
            set
            {
                if (CurrentApplicationWindow == null) { return; }

                _ = CurrentApplicationWindow.Dispatcher.AwaitableRunAsync(() =>
                {
                    if (CurrentApplicationWindow.Content is FrameworkElement rootElement)
                    {
                        rootElement.RequestedTheme = value;
                    }

                    if (WindowHelper.IsSupported)
                    {
                        foreach (FrameworkElement element in WindowHelper.ActiveWindows.Keys.OfType<FrameworkElement>())
                        {
                            element.RequestedTheme = value;
                        }
                    }
                });

                SettingsHelper.Set(SettingsHelper.SelectedAppTheme, value);
                UpdateSystemCaptionButtonColors();
                InvokeUISettingChanged(IsDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light);
            }
        }

        public static void Initialize()
        {
            // Save reference as this might be null when the user is in another app
            CurrentApplicationWindow = Window.Current ?? App.MainWindow;
            RootTheme = SettingsHelper.Get<ElementTheme>(SettingsHelper.SelectedAppTheme);

            // Registering to color changes, thus we notice when user changes theme system wide
            UISettings = new UISettings();
            UISettings.ColorValuesChanged += UISettings_ColorValuesChanged;
        }

        public static void Initialize(Window window)
        {
            if (window?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = ActualTheme;
            }
            UpdateSystemCaptionButtonColors(window);
        }

        private static void UISettings_ColorValuesChanged(UISettings sender, object args)
        {
            UpdateSystemCaptionButtonColors();
            InvokeUISettingChanged(IsDarkTheme() ? ApplicationTheme.Dark : ApplicationTheme.Light);
        }

        public static bool IsDarkTheme()
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static bool IsDarkTheme(ElementTheme ActualTheme)
        {
            return Window.Current != null
                ? ActualTheme == ElementTheme.Default
                    ? Application.Current.RequestedTheme == ApplicationTheme.Dark
                    : ActualTheme == ElementTheme.Dark
                : ActualTheme == ElementTheme.Default
                    ? UISettings?.GetColorValue(UIColorType.Background) == Colors.Black
                    : ActualTheme == ElementTheme.Dark;
        }

        public static async void UpdateSystemCaptionButtonColors()
        {
            bool isDark = IsDarkTheme();
            bool isHighContrast = new AccessibilitySettings().HighContrast;

            Color foregroundColor = isDark || isHighContrast ? Colors.White : Colors.Black;
            Color backgroundColor = isHighContrast ? Color.FromArgb(255, 0, 0, 0) : isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            await (CurrentApplicationWindow?.Dispatcher).ResumeForegroundAsync();

            if (UIHelper.HasStatusBar)
            {
                StatusBar StatusBar = StatusBar.GetForCurrentView();
                StatusBar.ForegroundColor = foregroundColor;
                StatusBar.BackgroundColor = backgroundColor;
                StatusBar.BackgroundOpacity = 0; // 透明度
            }

            bool extendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = extendViewIntoTitleBar ? Colors.Transparent : backgroundColor;

            if (WindowHelper.IsSupported)
            {
                foreach (AppWindow window in WindowHelper.ActiveWindows.Values)
                {
                    bool ExtendsContentIntoTitleBar = window.TitleBar.ExtendsContentIntoTitleBar;
                    AppWindowTitleBar appTitleBar = window.TitleBar;
                    appTitleBar.ForegroundColor = appTitleBar.ButtonForegroundColor = foregroundColor;
                    appTitleBar.BackgroundColor = appTitleBar.InactiveBackgroundColor = backgroundColor;
                    appTitleBar.ButtonBackgroundColor = appTitleBar.ButtonInactiveBackgroundColor = ExtendsContentIntoTitleBar ? Colors.Transparent : backgroundColor;
                }
            }
        }

        public static async void UpdateSystemCaptionButtonColors(Window window)
        {
            await window.Dispatcher.ResumeForegroundAsync();

            bool isDark = window?.Content is FrameworkElement rootElement ? IsDarkTheme(rootElement.RequestedTheme) : IsDarkTheme();
            bool isHighContrast = new AccessibilitySettings().HighContrast;

            Color foregroundColor = isDark || isHighContrast ? Colors.White : Colors.Black;
            Color backgroundColor = isHighContrast ? Color.FromArgb(255, 0, 0, 0) : isDark ? Color.FromArgb(255, 32, 32, 32) : Color.FromArgb(255, 243, 243, 243);

            if (UIHelper.HasStatusBar)
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();
                statusBar.ForegroundColor = foregroundColor;
                statusBar.BackgroundColor = backgroundColor;
                statusBar.BackgroundOpacity = 0; // 透明度
            }

            bool extendViewIntoTitleBar = CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar;
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.ForegroundColor = titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.BackgroundColor = titleBar.InactiveBackgroundColor = backgroundColor;
            titleBar.ButtonBackgroundColor = titleBar.ButtonInactiveBackgroundColor = extendViewIntoTitleBar ? Colors.Transparent : backgroundColor;
        }
    }
}
