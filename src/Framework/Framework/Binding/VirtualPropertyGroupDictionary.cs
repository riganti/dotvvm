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
    /// <summary> Represents a dictionary of values of <see cref="DotvvmPropertyGroup" />. </summary>
    public readonly struct VirtualPropertyGroupDictionary<TValue> : IDictionary<string, TValue>, IReadOnlyDictionary<string, TValue>
    {
        private readonly DotvvmBindableObject control;
        private readonly DotvvmPropertyGroup group;

        public VirtualPropertyGroupDictionary(DotvvmBindableObject control, DotvvmPropertyGroup group)
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

        /// <summary> Lists all values. If any of the properties contains a binding, it will be automatically evaluated. </summary>
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

        public int Count
        {
            get
            {
                // we don't want to use Linq Enumerable.Count() as it would allocate
                // the enumerator. foreach gets the struct enumerator so it does not allocate anything
                var count = 0;
                foreach (var (p, _) in control.properties)
                {
                    var pg = p as GroupedDotvvmProperty;
                    if (pg != null && pg.PropertyGroup == group)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public bool Any()
        {
            foreach (var (p, _) in control.properties)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsReadOnly => false;

        ICollection<string> IDictionary<string, TValue>.Keys => Keys.ToList();

        ICollection<TValue> IDictionary<string, TValue>.Values => Values.ToList();

        /// <summary> Gets or sets value of property identified by <paramref name="key"/>. If the property contains a binding, the getter will automatically evaluate it. </summary>
        public TValue this[string key]
        {
            get
            {
                var p = group.GetDotvvmProperty(key);
                if (control.properties.TryGet(p, out var value))
                    return (TValue)control.EvalPropertyValue(p, value)!;
                else
                    return (TValue)p.DefaultValue!;
            }
            set
            {
                control.properties.Set(group.GetDotvvmProperty(key), value);
            }
        }

        /// <summary> Gets the value binding set to a specified property. Returns null if the property is not a binding, throws if the binding some kind of command. </summary>
        public IValueBinding? GetValueBinding(string key) => control.GetValueBinding(group.GetDotvvmProperty(key));
        /// <summary> Gets the binding set to a specified property. Returns null if the property is not set or if the value is not a binding. </summary>
        public IBinding? GetBinding(string key) => control.GetBinding(group.GetDotvvmProperty(key));
        /// <summary> Gets the value or a binding object for a specified property. </summary>
        public object? GetValueRaw(string key)
        {
            var p = group.GetDotvvmProperty(key);
            if (control.properties.TryGet(p, out var value))
                return value;
            else
                return p.DefaultValue!;
        }

        /// <summary> Adds value or overwrites the property identified by <paramref name="key"/>. </summary>
        public void Set(string key, ValueOrBinding<TValue> value)
        {
            control.properties.Set(group.GetDotvvmProperty(key), value.UnwrapToObject());
        }
        /// <summary> Adds value or overwrites the property identified by <paramref name="key"/> with the value. </summary>
        public void Set(string key, TValue value) =>
            control.properties.Set(group.GetDotvvmProperty(key), value);
        /// <summary> Adds binding or overwrites the property identified by <paramref name="key"/> with the binding. </summary>
        public void SetBinding(string key, IBinding binding) =>
            control.properties.Set(group.GetDotvvmProperty(key), binding);

        public bool ContainsKey(string key)
        {
            return control.Properties.ContainsKey(group.GetDotvvmProperty(key));
        }

        private void AddOnConflict(GroupedDotvvmProperty property, object? value)
        {
            var merger = this.group.ValueMerger;
            if (merger is null)
                throw new ArgumentException($"Cannot Add({property.Name}, {value}) since the value is already set and merging is not enabled on this property group.");
            var mergedValue = merger.MergePlainValues(property, control.properties.GetOrThrow(property), value);
            control.properties.Set(property, mergedValue);
        }

        /// <summary> Adds the property identified by <paramref name="key"/>. If the property is already set, it tries appending the value using the group's <see cref="Compilation.IAttributeValueMerger" /> </summary>
        public void Add(string key, ValueOrBinding<TValue> value)
        {
            var prop = group.GetDotvvmProperty(key);
            object? val = value.UnwrapToObject();
            if (!control.properties.TryAdd(prop, val))
                AddOnConflict(prop, val);
        }

        /// <summary> Adds the property identified by <paramref name="key"/>. If the property is already set, it tries appending the value using the group's <see cref="Compilation.IAttributeValueMerger" /> </summary>
        public void Add(string key, TValue value) =>
            this.Add(key, new ValueOrBinding<TValue>(value));

        /// <summary> Adds the property identified by <paramref name="key"/>. If the property is already set, it tries appending the value using the group's <see cref="Compilation.IAttributeValueMerger" /> </summary>
        public void AddBinding(string key, IBinding? binding)
        {
            Add(key, new ValueOrBinding<TValue>(binding!));
        }

        public void CopyFrom(IEnumerable<KeyValuePair<string, TValue>> values, bool clear = false)
        {
            if (clear) this.Clear();
            foreach (var item in values)
            {
                Set(item.Key, item.Value);
            }
        }

        public void CopyFrom(IEnumerable<KeyValuePair<string, ValueOrBinding<TValue>>> values, bool clear = false)
        {
            if (clear) this.Clear();
            foreach (var item in values)
            {
                Set(item.Key, item.Value);
            }
        }
        public static IDictionary<string, TValue> CreateValueDictionary(DotvvmBindableObject control, DotvvmPropertyGroup group)
        {
            var result = new Dictionary<string, TValue>();
            foreach (var (p, valueRaw) in control.properties)
            {
                if (p is GroupedDotvvmProperty pg && pg.PropertyGroup == group)
                {
                    var valueObj = control.EvalPropertyValue(p, valueRaw);
                    if (valueObj is TValue value)
                        result.Add(pg.GroupMemberName, value);
                    else if (valueObj is null)
                        result.Add(pg.GroupMemberName, default!);
                }
            }
            return result;
        }

        public static IDictionary<string, ValueOrBinding<TValue>> CreatePropertyDictionary(DotvvmBindableObject control, DotvvmPropertyGroup group)
        {
            var result = new Dictionary<string, ValueOrBinding<TValue>>();
            foreach (var (p, valRaw) in control.properties)
            {
                if (p is GroupedDotvvmProperty pg && pg.PropertyGroup == group)
                {
                    result.Add(pg.GroupMemberName, ValueOrBinding<TValue>.FromBoxedValue(valRaw));
                }
            }
            return result;
        }
        public bool Remove(string key)
        {
            return control.Properties.Remove(group.GetDotvvmProperty(key));
        }

        /// <summary> Tries getting value of property identified by <paramref name="key"/>. If the property contains a binding, it will be automatically evaluated. </summary>
#pragma warning disable CS8767
        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
#pragma warning restore CS8767
        {
            var prop = group.GetDotvvmProperty(key);
            if (control.properties.TryGet(prop, out var valueRaw))
            {
                value = (TValue)control.EvalPropertyValue(prop, valueRaw)!;
                return true;
            }
            else
            {
                value = default(TValue)!;
                return false;
            }
        }

        /// <summary> Adds the property-value pair to the dictionary. If the property is already set, it tries appending the value using the group's <see cref="Compilation.IAttributeValueMerger" /> </summary>
        public void Add(KeyValuePair<string, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            // we want to avoid allocating the list if there is only one property
            DotvvmProperty? toRemove = null;
            List<DotvvmProperty>? toRemoveRest = null;

            foreach (var (p, _) in control.properties)
            {
                var pg = p as GroupedDotvvmProperty;
                if (pg != null && pg.PropertyGroup == group)
                {
                    if (toRemove is null)
                        toRemove = p;
                    else
                    {
                        if (toRemoveRest is null)
                            toRemoveRest = new List<DotvvmProperty>();
                        toRemoveRest.Add(p);
                    }
                }
            }

            if (toRemove is {})
                control.Properties.Remove(toRemove);

            if (toRemoveRest is {})
                foreach (var p in toRemoveRest)
                    control.Properties.Remove(p);
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

        /// <summary> Enumerates all keys and values. If a property contains a binding, the it will be automatically evaluated. </summary>
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

        /// <summary> Enumerates all keys and values, without evaluating the bindings. </summary>
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
