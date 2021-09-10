using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Binding;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Serialization
{
    public class SerializationViewModel : DotvvmViewModelBase
    {

        [Bind(Direction.ServerToClient)]
        public string Value { get; set; }

        [Bind(Direction.ClientToServer)]
        public string Value2 { get; set; }

        public string Results { get; set; }

        public string IgnoredProperty { get; set; } = "";


        public void Test()
        {
            Results = Value + "," + Value2;

            Value2 = "TEST"; // this will not go to the client
        }

    }
}