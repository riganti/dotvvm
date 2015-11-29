using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class PostBackHandlersViewModel : DotvvmViewModelBase
    {
        public int LastCommandValue { get; set; }

        public List<MessageDate> Generated { get; set; } = new List<MessageDate>()
        {
            new MessageDate() { Message = "Generated 1", Value = 4 },
            new MessageDate() { Message = "Generated 2", Value = 5 }
        };

        public void DoWork(int value)
        {
            LastCommandValue = value;
        }

    }
    public class MessageDate
    {
        public string Message { get; set; }

        public int Value { get; set; }
    }
}
