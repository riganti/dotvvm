using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
    public class ControlControlCommandInvokeActionViewModel : DotvvmViewModelBase
    {
        public List<int> Rows { get; set; }
        public List<int> Columns { get; set; }
        public string Value { get; set; }

        public override Task Load()
        {
            Rows = new List<int>
            {
                1,2,3
            };
            Columns = new List<int>
            {
                5,4
            };

            return base.Load();
        }

        public void OnGoToDetail(int id1, int id2)
        {

            Value = id1 + "|" + id2;
        }
    }
}

