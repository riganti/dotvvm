using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostbackAbortSignal
{
    public class LoadAbortViewModel : DotvvmViewModelBase
    {
        [AllowStaticCommand]
        public static async Task<string[]> LoadDataAsync()
        {
            await Task.Delay(2000);
            return new[] { "strawberry", "lemon", "mango" };
        }

        public static string[] LoadData()
        {
            Thread.Sleep(2000);
            return new[] { "strawberry", "lemon", "mango" };
        }

        public string[] Data { get; set; } = new string[] {};
    }
}

