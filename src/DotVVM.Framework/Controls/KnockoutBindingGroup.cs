#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.ViewModel.Serialization;
using Newtonsoft.Json;

namespace DotVVM.Framework.Controls
{
    public class KnockoutBindingGroup: IEnumerable<KnockoutBindingGroup.KnockoutBindingInfo>
    {

        private List<KnockoutBindingInfo> entries = new List<KnockoutBindingInfo>();
        
        public bool IsEmpty => entries.Count == 0;

        public virtual void Add(string name, DotvvmControl control, DotvvmProperty property, Action? nullBindingAction = null)
        {
            var binding = control.GetValueBinding(property);
            if (binding == null)
            {
                if (nullBindingAction != null) nullBindingAction();
                else Add(name, JsonConvert.SerializeObject(control.GetValue(property), DefaultViewModelSerializer.CreateDefaultSettings()));
            }
            else
            {
                entries.Add(new KnockoutBindingInfo(name, GetKnockoutBindingExpression(control, binding)));
            }
        }

        public virtual void Add(string name, string expression, bool surroundWithDoubleQuotes = false)
        {
            if (surroundWithDoubleQuotes)
            {
                expression = JsonConvert.SerializeObject(expression);
            }

            entries.Add(new KnockoutBindingInfo(name, expression));
        }

        public virtual void AddFrom(KnockoutBindingGroup other)
        {
            entries.AddRange(other.entries);
        }

        public List<KnockoutBindingInfo>.Enumerator GetEnumerator() => entries.GetEnumerator();

        protected virtual string GetKnockoutBindingExpression(DotvvmBindableObject obj, IValueBinding valueBinding)
        {
            return valueBinding.GetKnockoutBindingExpression(obj);
        }

        public override string ToString()
        {
            if (entries.Count == 0) return "{}";
            return "{ " + string.Join(", ", entries) + " }";
        }

        IEnumerator<KnockoutBindingInfo> IEnumerable<KnockoutBindingInfo>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public class KnockoutBindingInfo
        {
            public KnockoutBindingInfo(string name, string expression)
            {
                Name = name;
                Expression = expression;
            }

            public string Name { get; }
            public string Expression { get; }

            public override string ToString()
            {
                return "'" + Name + "': " + Expression;
            }
        }
    }
}
