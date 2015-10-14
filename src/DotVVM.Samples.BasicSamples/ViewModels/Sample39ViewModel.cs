using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample39ViewModel : DotvvmViewModelBase
    {
        public List<int> List { get; set; } = new List<int>() { 45 } ;
    }
}