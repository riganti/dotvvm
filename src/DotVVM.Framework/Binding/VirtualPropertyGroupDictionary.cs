#nullable enable
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Utils;
using System.Diagnostics.CodeAnalysis;

namespace DotVVM.Framework.Binding
{
    public struct VirtualPropertyGroupDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyDictionary<string, TValue>
    {
        private readonly DotvvmBindableObject control;
        private readonly DotvvmPropertyGroup  group;

        public VirtualPropertyGroupDictionary(DotvvmBindableObject control, DotvvmPropertyGroup  group)
        {
            this.control = control;
            this.group = group;
        }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (var (p, _) in control.properties)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        yield return pg.GroupMemberName;
                    }
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                foreach (var (p, _) in control.properties)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        yield return (TValue)control.GetValue(p)!;
                    }
                }
            }
        }

        public IEnumerable<GroupedDotvvmProperty> Properties
        {
            get
            {
                foreach (var (p, _) in control.properties)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        yield return pg;
                    }
                }
            }
        }

        public int Count => Keys.Count();

        public bool IsReadOnly => false;

        ICollection<string> IDictionary<string, TValue>.Keys => Keys.ToList();

        ICollection<TValue> IDictionary<string, TValue>.Values => Values.ToList();

        public TValue this[string key]
        {
            get
            {
                return (TValue)control.GetValue(group.GetDotvvmProperty(key))!;
            }
            set
            {
                control.SetValue(group.GetDotvvmProperty(key), value);
            }
        }

        public IValueBinding? GetValueBinding(string key) => control.GetValueBinding(group.GetDotvvmProperty(key));
        public IBinding? GetBinding(string key) => control.GetBinding(group.GetDotvvmProperty(key));
        public object? GetValueRaw(string key) => control.GetValueRaw(group.GetDotvvmProperty(key));


        public bool ContainsKey(string key)
        {
            return control.IsPropertySet(group.GetDotvvmProperty(key));
        }

        public void Add(string key, TValue value)
        {
            control.SetValue(group.GetDotvvmProperty(key), value);
        }

        public void AddBinding(string key, IBinding binding)
        {
            control.SetBinding(group.GetDotvvmProperty(key), binding);
        }

        public bool Remove(string key)
        {
            return control.Properties.Remove(group.GetDotvvmProperty(key));
        }

#pragma warning disable CS8767
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore CS8767
        {
            var prop = group.GetDotvvmProperty(key);
            if (control.IsPropertySet(prop))
            {
                value = (TValue)control.GetValue(prop)!;
                return true;
            }
            else
            {
                value = default(TValue)!;
                return false;
            }
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var (p, _) in control.properties)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    control.Properties.Remove(p);
                }
            }
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
#pragma warning disable CS8717
            return TryGetValue(item.Key, out var realValue) && object.Equals(realValue, item.Value);
#pragma warning restore CS8717
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public void CopyTo(IDictionary<string, TValue> dictionary)
        {
            foreach (var item in this)
            {
                dictionary[item.Key] = item.Value;
            }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (Contains(item)) return Remove(item.Key);
            return false;
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach (var (p, value) in control.properties)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    yield return new KeyValuePair<string, TValue>(pg.GroupMemberName, (TValue)control.EvalPropertyValue(p, value)!);
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerable<KeyValuePair<string, object>> RawValues
        {
            get
            {
                foreach (var (p, value) in control.properties)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        yield return new KeyValuePair<string, object>(pg.GroupMemberName, value!);
                    }
                }
            }
        }
    }
}
