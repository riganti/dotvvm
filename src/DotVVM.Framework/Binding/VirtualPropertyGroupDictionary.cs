using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace DotVVM.Framework.Binding
{
    public struct VirtualPropertyGroupDictionary<TValue>: IDictionary<string, TValue>, IReadOnlyDictionary<string, TValue>
    {
        private readonly DotvvmBindableObject control;
        private readonly PropertyGroupDescriptor group;

        public VirtualPropertyGroupDictionary(DotvvmBindableObject control, PropertyGroupDescriptor group)
        {
            this.control = control;
            this.group = group;
        }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (var p in control.properties.Keys)
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
                foreach (var p in control.properties.Keys)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        yield return (TValue)control.GetValue(p);
                    }
                }
            }
        }

        public IEnumerable<GroupedDotvvmProperty> Properties
        {
            get
            {
                foreach (var p in control.properties.Keys)
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
            get {
                return (TValue)control.GetValue(group.GetDotvvmProperty(key));
            }
            set {
                control.SetValue(group.GetDotvvmProperty(key), value);
            }
        }


        public bool ContainsKey(string key)
        {
            return control.IsPropertySet(group.GetDotvvmProperty(key));
        }

        public void Add(string key, TValue value)
        {
            control.SetValue(group.GetDotvvmProperty(key), value);
        }

        public bool Remove(string key)
        {
            return control.properties.Remove(group.GetDotvvmProperty(key));
        }

        public bool TryGetValue(string key, out TValue value)
        {
            var prop = group.GetDotvvmProperty(key);
            if(control.IsPropertySet(prop))
            {
                value = (TValue)control.GetValue(prop);
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            foreach (var p in control.properties.Keys)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    control.properties.Remove(p);
                }
            }
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            TValue realValue;
            return TryGetValue(item.Key, out realValue) && realValue.Equals(item);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            foreach (var item in this)
            {
                array[arrayIndex++] = item;
            }
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            if (Contains(item)) return Remove(item.Key);
            return false;
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            foreach (var p in control.properties.Keys)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    yield return new KeyValuePair<string, TValue>(pg.Name, (TValue)control.GetValue(p));
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
