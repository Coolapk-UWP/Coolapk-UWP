using Windows.UI.Xaml.Automation.Peers;

namespace CoolapkUWP.Controls
{
    /// <summary>
    /// AutomationPeer for SettingExpander
    /// </summary>
    public class SettingExpanderAutomationPeer : ItemsControlAutomationPeer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingExpander"/> class.
        /// </summary>
        /// <param name="owner">SettingExpander</param>
        public SettingExpanderAutomationPeer(SettingExpander owner) : base(owner)
        {
        }

        /// <summary>
        /// Gets the control type for the element that is associated with the UI Automation peer.
        /// </summary>
        /// <returns>The control type.</returns>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Button;
        }

        /// <summary>
        /// Called by GetClassName that gets a human readable name that, in addition to AutomationControlType,
        /// differentiates the control represented by this AutomationPeer.
        /// </summary>
        /// <returns>The string that contains the name.</returns>
        protected override string GetClassNameCore()
        {
            string classNameCore = Owner.GetType().Name;
#if DEBUG_AUTOMATION
            System.Diagnostics.Debug.WriteLine("SettingsCardAutomationPeer.GetClassNameCore returns " + classNameCore);
#endif
            return classNameCore;
        }
    }
}
