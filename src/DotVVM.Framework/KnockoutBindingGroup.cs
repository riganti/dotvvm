using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework
{
    public class KnockoutBindingGroup
    {

        private List<KnockoutBindingInfo> info = new List<KnockoutBindingInfo>();
        
        public bool IsEmpty => info.Count == 0;

        public void Add(string name, DotvvmBindableControl control, DotvvmProperty property, Action nullBindingAction)
        {
            var binding = control.GetValueBinding(property);
            if (binding == null)
            {
                nullBindingAction();
            }
            else
            {
                info.Add(new KnockoutBindingInfo() { Name = name, Control = control, Property = property });
            }
        }


        public override string ToString()
        {
            return "{ " + string.Join(", ", info) + " }";
        }


        class KnockoutBindingInfo
        {
            public string Name { get; set; }
            public DotvvmBindableControl Control { get; set; }
            public DotvvmProperty Property { get; set; }

            public override string ToString()
            {
                return "\"" + Name + "\": " + Control.GetValueBinding(Property).TranslateToClientScript(Control, Property);
            }
        }

    }
}