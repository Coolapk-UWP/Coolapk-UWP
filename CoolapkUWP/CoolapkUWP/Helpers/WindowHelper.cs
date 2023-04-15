using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

namespace CoolapkUWP.Helpers
{
    // Helper class to allow the app to find the Window that contains an
    // arbitrary UIElement (GetWindowForElement). To do this, we keep track
    // of all active Windows. The app code must call WindowHelper.CreateWindow
    // rather than "new Window" so we can keep track of all the relevant
    // windows. In the future, we would like to support this in platform APIs.
    public static class WindowHelper
    {
        public static bool IsSupportedAppWindow => ApiInformation.IsTypePresent("Windows.UI.WindowManagement.AppWindow");

        public static bool IsAppWindow(this UIElement element) => IsSupportedAppWindow && element?.XamlRoot?.Content != null && ActiveWindows.ContainsKey(element.XamlRoot.Content);

        public static async Task<(AppWindow, Frame)> CreateWindow()
        {
            Frame newFrame = new Frame();
            AppWindow newWindow = await AppWindow.TryCreateAsync();
            ElementCompositionPreview.SetAppWindowContent(newWindow, newFrame);
            TrackWindow(newWindow, newFrame);
            return (newWindow, newFrame);
        }

        public static void TrackWindow(this AppWindow window, Frame frame)
        {
            window.Closed += (sender, args) =>
            {
                ActiveWindows?.Remove(frame);
                frame.Content = null;
                window = null;
            };
            ActiveWindows?.Add(frame, window);
        }

        public static AppWindow GetWindowForElement(this UIElement element)
        {
            return element.IsAppWindow() ? ActiveWindows[element.XamlRoot.Content] : null;
        }

        public static void SetXAMLRoot(this UIElement element, UIElement target)
        {
            if (ApiInformation.IsPropertyPresent("Windows.UI.Xaml.UIElement", "XamlRoot"))
            {
                element.XamlRoot = target?.XamlRoot;
            }
        }

        public static Dictionary<UIElement, AppWindow> ActiveWindows { get; } = IsSupportedAppWindow ? new Dictionary<UIElement, AppWindow>() : null;
    }
}
