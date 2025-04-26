using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Timer
{
	public class LongCommandViewModel : DotvvmViewModelBase
	{
		public int Value { get; set; }

        public async Task LongCommand()
        {
            await Task.Delay(3000);
            Value++;
        }
	}
}

