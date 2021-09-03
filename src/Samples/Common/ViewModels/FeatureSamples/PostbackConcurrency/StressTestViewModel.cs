using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostbackConcurrency
{
    public class StressTestViewModel : DotvvmViewModelBase
    {

        public int Value { get; set; }

        [Bind(Direction.ServerToClientFirstRequest)]
        public int RejectedCount { get; set; }

        [Bind(Direction.ServerToClientFirstRequest)]
        public int BeforePostback { get; set; }

        [Bind(Direction.ServerToClientFirstRequest)]
        public int AfterPostback { get; set; }

        public void Increment()
        {
            Value++;
            Thread.Sleep(0);
        }

    }
}
