using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
    public class DisableButtonPostbackHandler : PostBackHandler
    {
        protected internal override string ClientHandlerName => "disableButton";

        public bool Renable
        {
            get { return (bool)GetValue(RenableProperty); }
            set { SetValue(RenableProperty, value); }
        }
        public static readonly DotvvmProperty RenableProperty
            = DotvvmProperty.Register<bool, DisableButtonPostbackHandler>(c => c.Renable, true);

        protected internal override Dictionary<string, string> GetHandlerOptionClientExpressions() => new Dictionary<string, string>() { ["reenable"] = Renable.ToString().ToLower() };
    }
}
