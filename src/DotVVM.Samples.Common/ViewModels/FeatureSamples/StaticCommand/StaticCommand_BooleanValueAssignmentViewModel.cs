using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.Common.Views.FeatureSamples.StaticCommand;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_ValueAssignmentInControlViewModel : DotvvmViewModelBase
    {

        public StaticCommand_ValueAssignmentControlModel Model { get; set; } = new StaticCommand_ValueAssignmentControlModel();
     }
}

