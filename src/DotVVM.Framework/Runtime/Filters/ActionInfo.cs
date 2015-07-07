using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Runtime.Filters
{
    public class ActionInfo
    {

        public MethodInfo MethodInfo { get; internal set; }

        public ActionParameterInfo[] Arguments { get; internal set; }

        public bool IsControlCommand { get; internal set; }

        public object Target { get; internal set; }


        public Action GetAction()
        {
            return () => MethodInfo.Invoke(Target, Arguments.Select(a => a.Value).ToArray());
        }
    }
}