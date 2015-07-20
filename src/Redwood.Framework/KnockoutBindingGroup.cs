using System;
using System.Collections.Generic;
using Redwood.Framework.Binding;
using Redwood.Framework.Controls;

namespace Redwood.Framework
{
    public class KnockoutBindingGroup
    {

        private List<KnockoutBindingInfo> info = new List<KnockoutBindingInfo>(); 

        public void Add(string name, RedwoodBindableControl control, RedwoodProperty property, Action nullBindingAction)
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
            public RedwoodBindableControl Control { get; set; }
            public RedwoodProperty Property { get; set; }

            public override string ToString()
            {
                return "\"" + Name + "\": " + Control.GetValueBinding(Property).TranslateToClientScript(Control, Property);
            }
        }

    }
}