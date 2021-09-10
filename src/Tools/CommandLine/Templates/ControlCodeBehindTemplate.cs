namespace DotVVM.CommandLine.Templates
{
    public static class ControlCodeBehindTemplate
    {
        public static string TransformText(string @namespace, string name)
        {
            return
$@"
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.Controls;

namespace {@namespace}
{{
    public class {name} : DotvvmMarkupControl
    {{
    }}
}}
";
        }
    }
}
