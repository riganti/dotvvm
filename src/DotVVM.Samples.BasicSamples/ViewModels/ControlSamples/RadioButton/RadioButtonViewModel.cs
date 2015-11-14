using DotVVM.Framework.ViewModel;
using System.Collections.Generic;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.RadioButton
{
    public class RadioButtonViewModel : DotvvmViewModelBase
    {

        public int DBRB { get; set; }

        public IEnumerable<DRBChoice> DBRBChoices
        {
            get
            {
                return new DRBChoice[]
                {
                    new DRBChoice{ Id = 1, Title = "One" },
                    new DRBChoice{ Id = 2, Title = "Two" },
                    new DRBChoice{ Id = 3, Title = "Three" },
                    new DRBChoice{ Id = 4, Title = "Four" },
                    new DRBChoice{ Id = 5, Title = "Five" }
                };
            }
        }

        public class DRBChoice
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }

        public int DBRBResult { get; set; }

        public void UpdateDBRB()
        {
            DBRBResult = DBRB;
        }
    }
}