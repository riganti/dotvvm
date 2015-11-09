using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample32ViewModel
    {

        public string ButtonText => "Hello!";

        public bool Enabled { get; set; }

        public int Counter { get; set; }

        public void Switch()
        {
            Counter++;
            Enabled = !Enabled;
        }

    }
}