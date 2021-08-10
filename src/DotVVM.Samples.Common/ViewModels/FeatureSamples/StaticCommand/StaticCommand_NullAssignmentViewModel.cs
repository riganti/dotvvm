using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class StaticCommand_NullAssignmentViewModel : DotvvmViewModelBase
    {

        public int? IntItem1 { get; set; } = 10;
        public int? IntItem2 { get; set; } = 10;


        public DateTime? DateTimeItem1 { get; set; } = DateTime.Today;
        public DateTime? DateTimeItem2 { get; set; } = DateTime.Today;


        public string StringItem1 { get; set; } = "a";
        public string StringItem2 { get; set; } = "a";


        public ComplexType ObjectItem1 { get; set; } = new ComplexType();
        public ComplexType ObjectItem2 { get; set; } = new ComplexType();


        public class ComplexType
        {
        }

    }

}

