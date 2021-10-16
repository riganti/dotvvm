using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.TemplateHost
{
    public class BasicViewModel : DotvvmViewModelBase
    {

        public List<IntValue> ObjectList { get; set; } = new List<IntValue>()
        {
            new IntValue() { Value = 1 },
            new IntValue() { Value = 2 },
            new IntValue() { Value = 3 }
        };

        public IntValue CreateObject()
        {
            return new IntValue() { Value = 1 };
        }
    }

    public class IntValue
    {
        public int Value { get; set; }
    }
}

