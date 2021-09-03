using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.BindingContexts
{
    public class CollectionContextViewModel : DotvvmViewModelBase
    {
        public List<string> Texts { get; set; } = new List<string>
        {
            "Zeroth",
            "First",
            "Second",
            "Third",
            "Fourth",
            "Fifth",
            "Sixth",
            "Seventh",
            "Eighth",
            "Ninth",
            "Tenth",
            "Eleventh",
            "Twelfth"
        };

        [FromQuery("renderMode")]
        public DotVVM.Framework.Controls.RenderMode RenderMode { get; set; } = DotVVM.Framework.Controls.RenderMode.Client;
    }
}
