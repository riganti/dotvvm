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

        public List<int> Rows2 { get; set; }
        public List<int> Columns2 { get; set; }
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
                4,5
            };
            Rows2 = new List<int>
            {
                6,7,8,9
            };
            Columns2 = new List<int>
            {
                10,11,12,13,14
            };
            return base.Load();
        }

        public void OnGoToDetail(int id1, int id2, int id3, int id4)
        {
            Value = id1 + "|" + id2 + "|" + id3 + "|" + id4;
            Values.Add(Value);
        }

        public override Task PreRender()
        {
            IsAnyDuplicity = Values.Count != Values.Distinct().Count();
            return base.PreRender();
        }

        public List<string> Values { get; set; } = new List<string>();

        public bool IsAnyDuplicity { get; set; }
    }
}

