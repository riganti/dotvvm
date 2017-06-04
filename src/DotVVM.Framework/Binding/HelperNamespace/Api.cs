
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Compilation.Javascript.Ast;
using Newtonsoft.Json;

namespace DotVVM.Framework.Binding.HelperNamespace
{
    public static class Api
    {
        public static T RefreshOnChange<T>(T obj, object refreshOn) => obj;

        public static T RefreshOnEvent<T>(T obj, string eventName) => obj;
    }
}