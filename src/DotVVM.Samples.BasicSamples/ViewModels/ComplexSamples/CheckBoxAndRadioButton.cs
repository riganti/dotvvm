using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples
{
    public class CheckBoxAndRadioButtonViewModel
    {

        public bool SCB { get; set; }

        public string SCBResult { get; set; }

        public void UpdateSCB()
        {
            SCBResult = SCB.ToString();
        }


        public List<string> MCB { get; set; }

        public string MCBResult { get; set; }

        public void UpdateMCB()
        {
            MCBResult = string.Join(", ", MCB.Select(i => i.ToString()));
        }




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


        public CheckBoxAndRadioButtonViewModel()
        {
            MCB = new List<string>();
        }



        public bool ChangedValue { get; set; }

        public int NumberOfChanges { get; set; }

        public void OnChanged()
        {
            NumberOfChanges++;
        }
    }
}