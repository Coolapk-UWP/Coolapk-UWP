using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkUWP.Controls
{
    public sealed class SettingButton : Button
    {
        public SettingButton()
        {
            DefaultStyleKey = typeof(SettingButton);
            _ = RegisterPropertyChangedCallback(ContentProperty, OnContentChanged);
        }

        private void OnContentChanged(DependencyObject d, DependencyProperty dp)
        {
            SettingButton self = (SettingButton)d;
            if (self.Content != null)
            {
                if (self.Content.GetType() == typeof(Setting))
                {
                    Setting selfSetting = (Setting)self.Content;
                    ResourceDictionary Resource = new ResourceDictionary { Source = new Uri("ms-appx:///Controls/SettingButton/SettingButton.xaml") };
                    selfSetting.Style = (Style)Resource["ButtonContentSettingStyle"];

                    if (!string.IsNullOrEmpty(selfSetting.Header))
                    {
                        AutomationProperties.SetName(self, selfSetting.Header);
                    }
                }
            }
        }
    }
}
