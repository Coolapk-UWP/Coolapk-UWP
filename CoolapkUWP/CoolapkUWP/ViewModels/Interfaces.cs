using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    internal interface ICanComboBoxChangeSelectedIndex
    {
        List<string> ItemSource { get; }
        int ComboBoxSelectedIndex { get; }
        void SetComboBoxSelectedIndex(int value);
    }

    internal interface ICanToggleChangeSelectedIndex
    {
        bool ToggleIsOn { get; }
    }

    internal interface IViewModel : INotifyPropertyChanged
    {
        Task Refresh(bool reset);
        string Title { get; }
    }
}
