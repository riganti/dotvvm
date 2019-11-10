using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.CheckBox
{
    public class CheckedItemsRepeaterViewModel
    {
        public Outer Data { get; set; } = new Outer();

        public class Outer
        {
            public IList<Inner> AllData { get; set; }

            public IList<int> SelectedDataTestsIds { get; set; } = new List<int>();
        }

        public class Inner
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
