using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.ContentPlaceHolder
{
    public class DoubleContentPlaceHolderPage_ContentTestViewModel : DoubleContentPlaceHolderMasterPageViewModel
    {
        public DateTime Date { get; set; } = DateTime.Now;
    }
}

