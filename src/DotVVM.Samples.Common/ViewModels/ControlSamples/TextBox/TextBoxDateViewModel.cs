using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class TextBoxDateViewModel : DotvvmViewModelBase
    {
        public static DateTime TestedDate => new DateTime(2016,11,20,8,20,10,0);

        public DateTime DateBack { get; set; }
        public DateTime Date { get; set; }


        public override Task Load()
        {
            Date = TestedDate;
            return base.Load();
        }

        public void Send()
        {
            DateBack = Date;
        }
    }
}
