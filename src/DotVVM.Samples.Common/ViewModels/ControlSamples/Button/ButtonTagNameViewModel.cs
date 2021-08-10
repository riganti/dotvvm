using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Button
{
    public class ButtonTagNameViewModel
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