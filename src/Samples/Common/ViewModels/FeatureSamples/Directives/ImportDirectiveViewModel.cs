using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Directives
{
	public class ImportDirectiveViewModel : DotvvmViewModelBase
	{
        public static string Func() => "Hello from ImportDirectiveViewModel";

        public class NestedViewModel
        {
            public static string StaticText { get; set; } = "Hello From Nested Class";
        }
	}
}

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Directives1
{
    public class TestClass1
    {
        public static string Func() => "Hello TestClass1";
    }
}

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.DirectivesNonAlias
{
    public class TestClassNonAlias
    {
        public static string Func() => "Hello TestClassNonAlias";
    }
}



