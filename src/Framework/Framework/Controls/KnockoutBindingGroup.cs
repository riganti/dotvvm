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
    /// <summary> Represents a JS object holding multiple binding expressions or constant values. The binding handler will receive the JS object with the evaluated bindings, or the constants provided to the KnockoutBindingGroup.Add method. The group can either be passed to IHtmlWriter or converted into a string expression using <see cref="ToString()"/> </summary>
    public class KnockoutBindingGroup: IEnumerable<KnockoutBindingGroup.KnockoutBindingInfo>
    {
        private readonly List<KnockoutBindingInfo> entries = new List<KnockoutBindingInfo>();
        public bool IsEmpty => entries.Count == 0;
        public int Count => entries.Count;


        /// <summary> Adds value of the specified dotvvm <paramref name="property"/>. If the property contains a value binding, the JS expression is added to the group. Otherwise, the JSON serialized constant value is added. </summary>
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

        /// <summary> Adds the JS <paramref name="expression"/> to the group. </summary>
        public virtual void Add(string name, string expression)
        {
            entries.Add(new KnockoutBindingInfo(name, expression));
        }

        /// <summary> Adds another nested group. If the nested group is empty, nothing is added (undefined will be in the field). </summary>
        public void Add(string name, KnockoutBindingGroup nestedGroup)
        {
            if (!nestedGroup.IsEmpty)
                Add(name, nestedGroup.ToString());
        }

        /// <summary> Adds a knockout binding expression of the specified value binding. </summary>
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

        /// <summary> Adds the specified value as a JSON serialized constant expression. </summary>
        public virtual void AddValue(string name, object? value)
        {
            var expression = JsonConvert.SerializeObject(value, DefaultSerializerSettingsProvider.Instance.Settings);
            Add(name, expression);
        }

        /// <summary> Copies all fields from the <paramref name="other"/> binding group. </summary>
        public virtual void AddFrom(KnockoutBindingGroup other)
        {
            entries.AddRange(other.entries);
        }

        public List<KnockoutBindingInfo>.Enumerator GetEnumerator() => entries.GetEnumerator();

        protected virtual string GetKnockoutBindingExpression(DotvvmBindableObject obj, IValueBinding valueBinding)
        {
            return valueBinding.GetKnockoutBindingExpression(obj);
        }

        /// <summary> Returns the JS object expression with the <c>Add</c>ed fields. </summary>
        public override string ToString()
        {
            if (entries.Count == 0) return "{}";
            return "{ " + string.Join(", ", entries) + " }";
        }

        IEnumerator<KnockoutBindingInfo> IEnumerable<KnockoutBindingInfo>.GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary> A named expression - a field in the <see cref="KnockoutBindingGroup" /> </summary>
        public class KnockoutBindingInfo
        {
            public KnockoutBindingInfo(string name, string expression)
            {
                Name = name;
                Expression = expression;
            }

            /// <summary> The field name in the resulting JS object. </summary>
            public string Name { get; }
            /// <summary> JS expression returning the desired value. </summary>
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
