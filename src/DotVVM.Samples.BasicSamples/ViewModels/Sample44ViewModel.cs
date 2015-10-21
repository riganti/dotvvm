using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels
{
    public class Sample44ViewModel : DotvvmViewModelBase
    {
        //comboBox
        public string[] Fruits { get; set; } = { "Apple", "Banana", "Orange" };

        public string SelectedFruit { get; set; }

        public bool ComboEnabled { get; set; } = true;
        
        //TextBox
        public string Text { get; set; }

        public bool TextEnabled { get; set; } = true;

        //RadioButton
        public bool RadioButton1 { get; set; }

        public bool RadioButton2 { get; set; }

        public bool RadioEnabled { get; set; } = true;

        //CheckBox

        public bool CheckBoxChecked { get; set; }

        public bool CheckEnabled { get; set; } = true;

        public void DisableAll()
        {
            CheckEnabled = false;
            ComboEnabled = false;
            RadioEnabled = false;
            TextEnabled = false;
        }


    }
}