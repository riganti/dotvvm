using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.NamedCommand
{
    public class TestService
    {
        [AllowStaticCommand]
        public string Reverse(string s)
        {
            return new string(s.Reverse().ToArray());
        }
    }
}
