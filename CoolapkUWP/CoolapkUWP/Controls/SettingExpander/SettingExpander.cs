using Microsoft.UI.Xaml.Controls;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace CoolapkUWP.Controls
{
    public partial class SettingExpander : Expander
    {
        public SettingExpander()
        {
            DefaultStyleKey = typeof(Expander);
            Style = (Style)Application.Current.Resources["SettingExpanderStyle"];
            _ = RegisterPropertyChangedCallback(HeaderProperty, OnHeaderChanged);
        }

        private void OnHeaderChanged(DependencyObject d, DependencyProperty dp)
        {
            SettingExpander self = (SettingExpander)d;
            if (self.Header != null)
            {
                if (self.Header.GetType() == typeof(Setting))
                {
                    Setting selfSetting = (Setting)self.Header;
                    selfSetting.Style = (Style)Application.Current.Resources["ExpanderHeaderSettingStyle"];

                    if (!string.IsNullOrEmpty(selfSetting.Header))
                    {
                        AutomationProperties.SetName(self, selfSetting.Header);
                    }
                }
            }
        }
    }
}
