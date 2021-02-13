using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.LambdaExpressions
{
    public class LambdaExpressionsViewModel : DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.SPA.SiteViewModel
    {
        public int[] Array { get; set; } = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public int[] Result { get; set; }

        public void SetResult(IEnumerable<int> newResult)
        {
            Result = newResult.ToArray();
        }
    }
}
