using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// Represents a control that can contain multiple settings (or other) controls
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    public partial class SettingsGroup : ItemsControl
    {
        public SettingsGroup()
        {
            DefaultStyleKey = typeof(SettingsGroup);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            IsEnabledChanged -= SettingsGroup_IsEnabledChanged;
            OnHeaderChanged();
            SetEnabledState();
            IsEnabledChanged += SettingsGroup_IsEnabledChanged;
        }

        private void SettingsGroup_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetEnabledState();
        }

        private void SetEnabledState()
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", true);
        }

        /// <summary>
        /// Creates AutomationPeer
        /// </summary>
        /// <returns>An automation peer for <see cref="SettingsGroup"/>.</returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new SettingsGroupAutomationPeer(this);
        }

        public void OnHeaderChanged()
        {
            _ = VisualStateManager.GoToState(this, Header == null ? "HeaderCollapsed" : "HeaderVisible", false);
        }
    }
}
