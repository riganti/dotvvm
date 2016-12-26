using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Binding;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.MarkupControl
{
	public class ControlCommandBinding : DotvvmMarkupControl
	{
        public ControlCommandBindingDTO Data
        {
            get { return (ControlCommandBindingDTO)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public static readonly DotvvmProperty DataProperty
            = DotvvmProperty.Register<ControlCommandBindingDTO, ControlCommandBinding>(c => c.Data, null);

    }
}

