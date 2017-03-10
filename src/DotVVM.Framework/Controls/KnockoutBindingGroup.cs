using System;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    public class KnockoutBindingGroup
    {

        private List<KnockoutBindingInfo> entries = new List<KnockoutBindingInfo>();
        
        public bool IsEmpty => entries.Count == 0;

        public void Add(string name, DotvvmControl control, DotvvmProperty property, Action nullBindingAction = null)
        {
            var binding = control.GetValueBinding(property);
            if (binding == null)
            {
                if (nullBindingAction != null) nullBindingAction();
                else Add(name, JsonConvert.SerializeObject(control.GetValue(property)));
            }
            else
            {
                entries.Add(new KnockoutBindingInfo() { Name = name, Expression = control.GetValueBinding(property).GetKnockoutBindingExpression() });
            }
        }

        public void Add(string name, string expression, bool surroundWithDoubleQuotes = false)
        {
            if (surroundWithDoubleQuotes)
            {
                expression = JsonConvert.SerializeObject(expression);
            }

            entries.Add(new KnockoutBindingInfo() { Name = name, Expression = expression });
        }

        public void AddFrom(KnockoutBindingGroup other)
        {
            entries.AddRange(other.entries);
        }

        public override string ToString()
        {
            if (entries.Count == 0) return "{}";
            return "{ " + string.Join(", ", entries) + " }";
        }



        class KnockoutBindingInfo
        {
            public string Name { get; set; }
            public string Expression { get; set; }

            public override string ToString()
            {
                return "'" + Name + "': " + Expression;
            }
        }
    }
}