using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_TaskSequenceViewModel : DotvvmViewModelBase
    {
        public int Value { get; set; }

        public async Task Increment()
        {
            await Task.Delay(100);
            Value++;
        }

        public async Task Multiply()
        {
            await Task.Delay(100);
            Value *= 5;
        }

        [AllowStaticCommand]
        public static async Task<int> StaticIncrement(int value)
        {
            await Task.Delay(100);
            return value + 1;
        }

        [AllowStaticCommand]
        public static async Task<int> StaticMultiply(int value)
        {
            await Task.Delay(100);
            return value * 5;
        }
    }
}

