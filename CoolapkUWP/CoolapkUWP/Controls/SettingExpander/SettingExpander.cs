using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// The SettingExpander is a collapsable control to host multiple SettingsCards.
    /// </summary>
    public partial class SettingExpander : ItemsControl
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SettingExpander"/> class.
        /// </summary>
        public SettingExpander()
        {
            DefaultStyleKey = typeof(SettingExpander);
        }

        /// <inheritdoc />
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            RegisterAutomation();
        }

        private void RegisterAutomation()
        {
            if (Header is string headerString && headerString != string.Empty)
            {
                if (!string.IsNullOrEmpty(headerString) && string.IsNullOrEmpty(AutomationProperties.GetName(this)))
                {
                    AutomationProperties.SetName(this, headerString);
                }
            }
        }

        /// <summary>
        /// Creates AutomationPeer
        /// </summary>
        /// <returns>An automation peer for <see cref="SettingsExpander"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SettingExpanderAutomationPeer(this);
        }
    }
}
