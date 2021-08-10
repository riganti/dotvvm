using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.TextBox
{
    public class SimpleDateBoxViewModel : DotvvmViewModelBase
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string NameOfDay { get; set; }

        public void FillName()
        {
            this.NameOfDay = Date.DayOfWeek.ToString();
        }
    }
}