using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Redwood.Framework.ViewModel
{
    public class ViewModelPropertyValidationRule
    {

        public string ValidationRuleName { get; set; }

        public string ErrorMessage { get; set; }

        public JToken[] Parameters { get; set; }

    }
}