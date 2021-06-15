using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_ArrayAssignmentViewModel : DotvvmViewModelBase
    {
        public List<string> Names { get; set; } = new List<string> { "Anne", "Martin" };
        public List<string> DifferentNames { get; set; } = new List<string> { "Bob", "Oliver", "Pablo" };
    }
}

