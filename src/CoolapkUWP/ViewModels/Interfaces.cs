using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels
{
    internal interface ICanComboBoxChangeSelectedIndex
    {
        int ComboBoxSelectedIndex { get; }
        Task SetComboBoxSelectedIndex(int value);
    }

    internal interface IViewModel
    {
        double[] VerticalOffsets { get; set; }
        string Title { get; }
        Task Refresh(int p);
    }
}