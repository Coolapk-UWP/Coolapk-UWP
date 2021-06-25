using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoolapkUWP.ViewModels.MakeLikePage
{
    internal class ViewModel : IViewModel
    {
        public double[] VerticalOffsets { get; set; } = new double[1];
        public string Title { get; protected set; } = string.Empty;

        internal ViewModel(string id, string branch)
        {
        }

        public Task Refresh(int p)
        {
            throw new NotImplementedException();
        }
    }
}
