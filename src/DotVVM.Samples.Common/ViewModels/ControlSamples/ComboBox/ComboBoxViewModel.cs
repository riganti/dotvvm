using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.ComboBox
{
    public class ComboBoxViewModel
    {
        public object SelectedValue { get; set; }
        public string SelectedText { get; set; }
        public IEnumerable<string> Texts => new[] { "A", "AA", "AAA", "AAAA" };
    }
}
