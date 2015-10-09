using System;

namespace DotVVM.VS2015Extension.Bases.Directives
{
    public class DirectiveValue
    {
        public string Value { get; set; }

        public DirectiveValue(string value)
        {
            Value = value;
        }
    }
}