using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class SpaNavigationToEmptyUrlViewModel : SamplesViewModel
    {

        public void Redirect()
        {
            Context.RedirectToRoute("Default");
        }

    }
}
