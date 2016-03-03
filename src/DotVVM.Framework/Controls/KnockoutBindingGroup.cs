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

        public void Add(string name, DotvvmControl control, DotvvmProperty property, Action nullBindingAction)
        {
            var binding = control.GetValueBinding(property);
            if (binding == null)
            {
                nullBindingAction();
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
            return "{ " + string.Join(", ", entries) + " }";
        }



        class KnockoutBindingInfo
        {
            public string Name { get; set; }
            public string Expression { get; set; }

            public override string ToString()
            {
                return "\"" + Name + "\": " + Expression;
            }
        }
    }
}