using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
	public class ValidationScopes2ViewModel : DotvvmViewModelBase
	{

        public int Result { get; set; }


        public ValidationScopes2ChildViewModel Child { get; set; } = new ValidationScopes2ChildViewModel();

        public void Test()
        {
            Result++;
        }

    }

    public class ValidationScopes2ChildViewModel
    {

        [Required]
        public string ValueCheckedOnClient { get; set; }

        [MinLength(5)]
        public string ValueCheckedOnServer { get; set; }


    }
}

