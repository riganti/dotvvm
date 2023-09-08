using System;
using System.Collections;
using System.Collections.Generic;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Configuration;
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
                else AddValue(name, control.GetValue(property));
            }
            else
            {
                entries.Add(new KnockoutBindingInfo(name, GetKnockoutBindingExpression(control, binding)));
            }
        }

        public virtual void Add(string name, string expression)
        {
            entries.Add(new KnockoutBindingInfo(name, expression));
        }

        public void Add(string name, KnockoutBindingGroup nestedGroup)
        {
            if (!nestedGroup.IsEmpty)
                Add(name, nestedGroup.ToString());
        }

        public void Add(string name, DotvvmBindableObject contextControl, IValueBinding binding)
        {
            var expression = binding.GetKnockoutBindingExpression(contextControl);
            Add(name, expression);
        }

        [Obsolete("Use Add or AddValue instead")]
        public virtual void Add(string name, string expression, bool surroundWithDoubleQuotes)
        {
            if (surroundWithDoubleQuotes)
                AddValue(name, expression);
            else
                Add(name, expression);
        }

        public virtual void AddValue(string name, object? value)
        {
            var expression = JsonConvert.SerializeObject(value, DefaultSerializerSettingsProvider.Instance.Settings);
            Add(name, expression);
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
                if (MayBeUnquoted(Name))
                    return Name + ": " + Expression;
                else
                    return JsonConvert.ToString(Name, '"', StringEscapeHandling.EscapeHtml) + ": " + Expression;
            }

            private static bool MayBeUnquoted(string s)
            {
                // keywords are not a problem in JS, ({ if: A }) is perfectly valid, for example
                if (s.Length == 0) return false;
                if (char.IsDigit(s[0])) return false;
                foreach (var c in s)
                {
                    if ('a' <= c && c <= 'z')
                        continue;
                    if ('A' <= c && c <= 'Z')
                        continue;
                    if ('0' <= c && c <= '9')
                        continue;
                    if ('_' == c)
                        continue;
                    return false;
                }
                return true;
            }
        }
    }
}
