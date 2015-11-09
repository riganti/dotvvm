using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample17ViewModel_B : Sample17ViewModel
    {

        public Sample1ViewModel Sample1 { get; set; }

        public Sample17ViewModel_B()
        {
            HeaderText = "Task List";
        }

        public override Task Init()
        {
            if (!Context.IsPostBack)
            {
                Sample1 = new Sample1ViewModel() { Context = Context };
                Sample1.Init();
            }
            return base.Init();
        }

        public void Redirect()
        {
            Context.Redirect("~/Sample17/A/15");
        }
    }
}