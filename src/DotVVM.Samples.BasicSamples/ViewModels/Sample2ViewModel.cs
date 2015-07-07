using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample2ViewModel
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

        public IEnumerable DBRBChoices
        {
            get
            {
                return new[]
                {
                    new { Id = 1, Title = "One" },
                    new { Id = 2, Title = "Two" },
                    new { Id = 3, Title = "Three" },
                    new { Id = 4, Title = "Four" },
                    new { Id = 5, Title = "Five" }
                };
            }
        }

        public int DBRBResult { get; set; }

        public void UpdateDBRB()
        {
            DBRBResult = DBRB;
        }


        public Sample2ViewModel()
        {
            MCB = new List<string>();
        }

    }
}