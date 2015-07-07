using System;
using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.ViewModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ClientCommandAttribute : Attribute
    {

        /// <summary>
        /// Gets the name of the function in Javascript.
        /// </summary>
        public string JavascriptFunctionName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientCommandAttribute"/> class.
        /// </summary>
        public ClientCommandAttribute(string javascriptFunctionName)
        {
            JavascriptFunctionName = javascriptFunctionName;
        }

    }
}