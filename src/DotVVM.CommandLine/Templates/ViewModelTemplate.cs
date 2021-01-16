namespace DotVVM.CommandLine.Templates
{
    public static class ViewModelTemplate
    {
        public static string TransformText(
            string @namespace,
            string name,
            string? @base)
        {
            return 
$@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace {@namespace}
{{
    public class {name} : {@base}
    {{

    }}
}}
";
        }
    }
}
