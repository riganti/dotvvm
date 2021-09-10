using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.Errors
{
    public class EncryptedPropertyInValueBindingViewModel : DotvvmViewModelBase
    {
        [Protect(ProtectMode.EncryptData)]
        public string SomeProperty { get; set; } = "Hello!";
    }
}
