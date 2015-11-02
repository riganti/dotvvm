using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample49ViewModel : DotvvmViewModelBase
    {

        public int LastCommandValue { get; set; }

        public List<Sample49Message> Generated { get; set; } = new List<Sample49Message>()
        {
            new Sample49Message() { Message = "Generated 1", Value = 4 },
            new Sample49Message() { Message = "Generated 2", Value = 5 }
        };

        public void DoWork(int value)
        {
            LastCommandValue = value;
        }

    }

    public class Sample49Message
    {
        public string Message { get; set; }

        public int Value { get; set; }
    }
}