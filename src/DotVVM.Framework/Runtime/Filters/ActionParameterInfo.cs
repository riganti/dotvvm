using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DotVVM.Framework.Runtime.Filters
{
    public class ActionParameterInfo
    {

        public string Name { get; internal set; }

        public Type Type { get; internal set; }

        public object Value { get; internal set; }

        public ParameterInfo ParameterInfo { get; internal set; }

    }
}