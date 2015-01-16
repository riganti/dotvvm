using System;
using System.Collections.Generic;
using System.Linq;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelValidationError
    {
        /// <summary>
        /// Contains path that can be evaluated on the client side.
        /// E.g.: Product().Suppliers()[2].Name
        /// </summary>
        public string PropertyPath { get; set; }

        public string ErrorMessage { get; set; }
    }
}