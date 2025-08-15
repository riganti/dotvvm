using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Utils;
using Impl = DotVVM.Framework.Controls.PropertyDictionaryImpl;

namespace DotVVM.Framework.Controls
{
    internal struct DotvvmControlProperties : IEnumerable<KeyValuePair<DotvvmPropertyId, object?>>
    {
        // There are 3 possible states of this structure:
        // 1. Empty -> keys == values == null
        // 2. Dictinary -> keys == null & values is Dictionary<DotvvmPropertyId, object> --> it falls back to traditional mutable property dictionary
        // 3. Array8 or Array16 -> keys is DotvvmPropertyId[] & values is object[] -- small linear search array

        // Note about unsafe code:
        // There is always the possibility that the structure may get into an invalid state, for example due to multithreded writes.
        // That is obviously not supported and it's a user error, and crashes are expected, but we want to avoid critical security issues like RCE.
        // The idea is that "reading random memory is OK-ish, writing is not", so we insert runtime type checks
        // to the places where this could happen:
        // * we write into keys, but it has insufficient length (8 instead of expected 16)
        // * we write into values, - it has insufficient length
        //                         - it is of different type (array vs dictionary)
        // * we return an invalid object reference and the client writes into it

        private const MethodImplOptions Inline = MethodImplOptions.AggressiveInlining;
        private const MethodImplOptions NoInlining = MethodImplOptions.NoInlining;


        private DotvvmPropertyId[]? keys;

        private object? values; // either object?[] or Dictionary<DotvvmPropertyId, object?>

        private TableState state;

        /// If the keys array is owned by this structure and can be modified. Not used in the dictionary mode.
        private bool ownsKeys;
        /// If the values array or the values Dictionary is owned by this structure and can be modified.
        private bool ownsValues;

        private readonly bool IsArrayState => ((byte)state | 1) == 3; // state == TableState.Array8 || state == TableState.Array16;

        
        private object?[] valuesAsArray
        {
            [MethodImpl(Inline)]
            readonly get
            {
                var value = this.values;
                Impl.Assert(value!.GetType() == typeof(object[])); // safety runtime check
                return Unsafe.As<object?[]>(value);
            }
            [MethodImpl(Inline)]
            set => this.values = value;
        }
        private readonly object?[]? valuesAsArrayUnsafe
        {
            [MethodImpl(Inline)]
            get => Unsafe.As<object?[]>(values);
        }

        private Dictionary<DotvvmPropertyId, object?> valuesAsDictionary
        {
            [MethodImpl(Inline)]
            readonly get
            {
                var value = this.values;
                Impl.Assert(value!.GetType() == typeof(Dictionary<DotvvmPropertyId, object?>)); // safety runtime check
                return Unsafe.As<Dictionary<DotvvmPropertyId, object?>>(value);
            }
            [MethodImpl(Inline)]
            set => this.values = value;
        }
        private readonly Dictionary<DotvvmPropertyId, object?>? valuesAsDictionaryUnsafe
        {
            [MethodImpl(Inline)]
            get => Unsafe.As<Dictionary<DotvvmPropertyId, object?>>(values);
        }

        [Conditional("DEBUG")]
        private readonly void CheckInvariant()
        {
            switch (state)
            {
                case TableState.Empty:
                    Debug.Assert(keys is null && values is null);
                    break;
                case TableState.Array8:
                case TableState.Array16:
                    Debug.Assert(keys is {});
                    Debug.Assert(values is object[]);
                    Debug.Assert(keys.Length == valuesAsArray.Length, $"keys.Length={keys.Length} != values.Length={valuesAsArray.Length}");
                    Debug.Assert(keys.Length == (state == TableState.Array8 ? 8 : 16));
                    for (int i = keys.Length - 1; i >= 0 ; i--)
                    {
                        var value = valuesAsArray[i];
                        var p = keys[i];
                        Debug.Assert(p.Id == 0 || keys.AsSpan().IndexOf(p) == i, $"Duplicate property {p} at index {i} and {keys.AsSpan().IndexOf(keys[i])}");
                        if (p.Id != 0)
                        {
                            // TODO: check currently causes issues in unrelated code
                            // var propType = p.PropertyType;
                            // Debug.Assert(value is null || value is IBinding || propType.IsInstanceOfType(value), $"Property {p} has value {value} of type {value?.GetType()} which is not assignable to {propType}");
                        }
                        else
                        {
                            Debug.Assert(valuesAsArray[i] is null, $"Zero property id at index {i} has non-null value: {valuesAsArray[i]}");
                        }
                    }
                    break;
                case TableState.Dictinary:
                    Debug.Assert(keys is null);
                    Debug.Assert(values is Dictionary<DotvvmPropertyId, object>);
                    break;
                default:
                    Impl.Fail();
                    break;
            }
        }

        public void AssignBulk(DotvvmPropertyId[] keys, object?[] values, bool ownsKeys, bool ownsValues)
        {
            if (keys is null || values is null || !(keys.Length is 8 or 16) || keys.Length != values.Length || values.GetType() != typeof(object[]))
                throwArgumentError(keys, values);

            CheckInvariant();
            if (this.values == null || Object.ReferenceEquals(this.keys, keys))
            {
                // empty -> fast assignment
                this.valuesAsArray = values;
                this.keys = keys;
                this.state = keys.Length switch {
                    8 => TableState.Array8,
                    16 => TableState.Array16,
                    _ => throw new NotSupportedException("Only 8 and 16 elements are supported")
                };
                this.ownsKeys = ownsKeys;
                this.ownsValues = ownsValues;
            }
            else
            {
                for (int i = 0; i < keys.Length; i++)
                {
                    if (keys[i].Id != 0)
                        this.Set(keys[i]!, values[i]);
                }
            }
            CheckInvariant();

            [DoesNotReturn, MethodImpl(NoInlining)]
            void throwArgumentError(DotvvmPropertyId[]? keys, object?[]? values)
            {
                ThrowHelpers.ArgumentNull(nameof(keys));
                ThrowHelpers.ArgumentNull(nameof(values));
                if (keys.Length is not 8 and not 16)
                    throw new ArgumentException($"The length of keys array must be 8 or 16.", nameof(keys));
                if (keys.Length != values.Length)
                    throw new ArgumentException($"The length of values array must be the same.", nameof(keys));
                // The our unsafe memory accesses are quite likely to mess up with array covariance, just make sure we don't encounter that
                if (values.GetType() != typeof(object[]))
                    throw new ArgumentException($"The values array must be of type {typeof(object[])}.", nameof(values));
                throw new Exception("Unknown error");
            }
        }

        public void AssignBulk(Dictionary<DotvvmPropertyId, object?> values, bool owns)
        {
            ThrowHelpers.ArgumentNull(values);

            CheckInvariant();
            if (this.values == null || object.ReferenceEquals(this.values, values))
            {
                this.keys = null;
                this.valuesAsDictionary = values;
                this.ownsValues = owns;
                this.state = TableState.Dictinary;
            }
            else
            {
                if (owns)
                {
                    foreach (var (k, v) in this)
                    {
                        values.TryAdd(k, v);
                    }
                    this.values = values;
                    this.keys = null;
                    this.ownsValues = true;
                }
                else
                {
                    foreach (var (k, v) in values)
                    {
                        this.Set(k, v);
                    }
                }
            }
        }

        public void ClearEverything()
        {
            CheckInvariant();
            values = null;
            keys = null;
            state = TableState.Empty;
        }

        public readonly bool Contains(DotvvmProperty p) => Contains(p.Id);
        public readonly bool Contains(DotvvmPropertyId p)
        {
            CheckInvariant();
            if (state == TableState.Array8)
            {
                Debug.Assert(values!.GetType() == typeof(object[]));
                Debug.Assert(keys is {});
                return Impl.ContainsKey8(this.keys, p);
            }
            return ContainsOutlined(p);
        }

        private readonly bool ContainsOutlined(DotvvmPropertyId p) // doesn't need to be inlined
        {
            if (state == TableState.Empty)
                return false;
            if (state == TableState.Array16)
            {
                return Impl.ContainsKey16(this.keys!, p);
            }
            if (state == TableState.Dictinary)
            {
                return valuesAsDictionary!.ContainsKey(p);
            }
            return Impl.Fail<bool>();
        }

        public readonly bool ContainsPropertyGroup(DotvvmPropertyGroup group) => ContainsPropertyGroup(group.Id);
        public readonly bool ContainsPropertyGroup(ushort groupId)
        {
            CheckInvariant();
            if (state == TableState.Array8)
            {
                return Impl.ContainsPropertyGroup(this.keys!, groupId);
            }
            return ContainsPropertyGroupOutlined(groupId);
        }

        private readonly bool ContainsPropertyGroupOutlined(ushort groupId)
        {
            if (state == TableState.Empty)
                return false;
            if (state == TableState.Array16)
            {
                return Impl.ContainsPropertyGroup(this.keys!, groupId);
            }
            else if (values is null) return false;
            else
            {
                Debug.Assert(values is Dictionary<DotvvmPropertyId, object>);
                foreach (var key in valuesAsDictionary.Keys)
                {
                    if (key.IsInPropertyGroup(groupId))
                        return true;
                }
                return false;
            }

        }

        public readonly int CountPropertyGroup(DotvvmPropertyGroup group) => CountPropertyGroup(group.Id);
        public readonly int CountPropertyGroup(ushort groupId)
        {
            CheckInvariant();
            if (state == TableState.Array8)
            {
                return Impl.CountPropertyGroup8(this.keys!, groupId);
            }
            return CountPropertyGroupOutlined(groupId);
        }

        private readonly int CountPropertyGroupOutlined(ushort groupId)
        {
            switch (state)
            {
                case TableState.Empty:
                    return 0;
                case TableState.Array16:
                    return Impl.CountPropertyGroup(this.keys!, groupId);
                case TableState.Dictinary:
                {
                    int count = 0;
                    foreach (var key in valuesAsDictionary.Keys)
                    {
                        if (key.IsInPropertyGroup(groupId))
                            count++;
                    }
                    return count;
                }
                default:
                    return Impl.Fail<int>();
            }
        }

        public void ClearPropertyGroup(ushort groupId)
        {
            switch (state)
            {
                case TableState.Empty:
                    return;
                case TableState.Array8:
                case TableState.Array16:
                {
                    var bitmap = Impl.FindGroupBitmap(this.keys!, groupId);
                    int index = 0;
                    if (bitmap == 0)
                        return;
                    OwnKeys();
                    OwnValues();
                    do
                    {
                        int bit = BitOperations.TrailingZeroCount(bitmap);
                        bitmap >>= (bit + 1);
                        index += bit;
                        Debug.Assert(keys![index].IsInPropertyGroup(groupId), $"Key {keys[index]} at index {index} is not in property group {groupId}");
                        keys![index] = default;
                        valuesAsArray[index] = null;
                        index++;
                    } while (bitmap != 0);
                    CheckInvariant();
                    return;
                }
                case TableState.Dictinary:
                {
                    var dict = this.valuesAsDictionary;
                    // we want to avoid allocating the list if there is only one property
                    DotvvmPropertyId toRemove = default;
                    List<DotvvmPropertyId>? toRemoveRest = null;

                    foreach (var (p, _) in dict)
                    {
                        if (p.IsInPropertyGroup(groupId))
                        {
                            if (toRemove.Id == 0)
                                toRemove = p;
                            else
                            {
                                toRemoveRest ??= new List<DotvvmPropertyId>();
                                toRemoveRest.Add(p);
                            }
                        }
                    }

                    if (toRemove.Id != 0)
                    {
                        if (!ownsValues)
                        {
                            CloneValues();
                            dict = this.valuesAsDictionary;
                        }

                        dict.Remove(toRemove);
                    }

                    if (toRemoveRest is {})
                    {
                        Debug.Assert(ownsValues);
                        foreach (var p in toRemoveRest)
                            dict.Remove(p);
                    }
                    CheckInvariant();
                    return;
                }
            }
            Impl.Fail();
        }

        [MethodImpl(Inline)]
        public readonly bool TryGet(DotvvmProperty p, out object? value) => TryGet(p.Id, out value);
        [MethodImpl(Inline)]
        public readonly bool TryGet(DotvvmPropertyId p, out object? value)
        {
            CheckInvariant();
            if (state == TableState.Array8)
            {
                var index = Impl.FindSlot8(this.keys!, p);
                Debug.Assert(index < 8);
                if (index >= 0)
                {
#if NET6_0_OR_GREATER
                    value = Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(valuesAsArray), index);
#else
                    value = valuesAsArray[index];
#endif
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
            return TryGetOutlined(p, out value);
        }
        private readonly bool TryGetOutlined(DotvvmPropertyId p, out object? value)
        {
            switch (state)
            {
                case TableState.Empty:
                    value = null;
                    return false;
                case TableState.Array16:
                {
                    var index = Impl.FindSlot16(this.keys!, p);
                    if (index >= 0)
                    {
                        value = valuesAsArray[index];
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }
                case TableState.Dictinary:
                    return valuesAsDictionary.TryGetValue(p, out value);
                default:
                    value = null;
                    return Impl.Fail<bool>();
            }
        }

        public readonly object? GetOrThrow(DotvvmProperty p) => GetOrThrow(p.Id);
        public readonly object? GetOrThrow(DotvvmPropertyId p)
        {
            if (this.TryGet(p, out var x)) return x;
            return ThrowKeyNotFound(p);
        }
        [MethodImpl(NoInlining), DoesNotReturn]
        private readonly object? ThrowKeyNotFound(DotvvmPropertyId p) => throw new KeyNotFoundException($"Property {p} was not found.");

        [MethodImpl(Inline)]
        public readonly object? GetOrNull(DotvvmProperty p) => GetOrNull(p.Id);
        [MethodImpl(Inline)]
        public readonly object? GetOrNull(DotvvmPropertyId p)
        {
            TryGet(p, out var x);
            return x;
        }

        [MethodImpl(Inline)]
        public void Set(DotvvmProperty p, object? value) => Set(p.Id, value);
        // not necessarily great for inlining
        public void Set(DotvvmPropertyId p, object? value)
        {
            CheckInvariant();
            if (p.MemberId == 0) ThrowZeroPropertyId();

            if (state == TableState.Array8)
            {
                var keys = this.keys!;
                var slot = Impl.FindSlotOrFree8(keys, p, out var exists);
                Debug.Assert(slot < 8);
                if (slot >= 0)
                {
                    if (!exists)
                    {
                        if (!ownsKeys)
                            keys = CloneKeys();
                        // arrays are always size >= 8
                        Unsafe.Add(ref Impl.UnsafeArrayReference(keys), slot) = p;
                    }
                    this.OwnValues();
                    Unsafe.Add(ref Impl.UnsafeArrayReference(this.valuesAsArray), slot) = value; // avoid covariance check
                    CheckInvariant();
                    Debug.Assert(GetOrThrow(p) == value, $"{p} was not set to {value}.");
                    return;
                }
            }

            SetOutlined(p, value);
        }
        private void SetOutlined(DotvvmPropertyId p, object? value)
        {
            TailCall:

            var keys = this.keys;
            if (keys is {})
            {
                Debug.Assert(values is object[]);
                var slot = state == TableState.Array8
                                ? Impl.FindSlotOrFree8(keys, p, out var exists)
                                : Impl.FindSlotOrFree16(keys, p, out exists);
                Debug.Assert(slot < 16);
                if (slot >= 0)
                {
                    Debug.Assert(slot < keys.Length && slot < valuesAsArray.Length, $"Slot {slot} is out of range for keys {keys.Length} and values {valuesAsArray.Length} (prop={p}, value={value})");
                    if (!exists)
                    {
                        if (!this.ownsKeys)
                            keys = CloneKeys();
                        OwnValues();
                        keys[slot] = p;
                        valuesAsArray[slot] = value;
                    }
                    else if (Object.ReferenceEquals(valuesAsArrayUnsafe![slot], value))
                    {
                        // no-op, we would be changing it to the same value
                    }
                    else
                    {
                        OwnValues();
                        valuesAsArray[slot] = value;
                    }
                }
                else
                {
                    IncreaseSize();
                    goto TailCall;
                }
            }
            else if (values == null)
            {
                SetEmptyToSingle(p, value);
            }
            else
            {
                Debug.Assert(values is Dictionary<DotvvmPropertyId, object?>);
                OwnValues();
                valuesAsDictionary[p] = value;
            }
            Debug.Assert(GetOrThrow(p) == value, $"{p} was not set to {value}.");
            CheckInvariant();
        }

        [MethodImpl(NoInlining), DoesNotReturn]
        private static void ThrowZeroPropertyId() => throw new ArgumentException("Invalid (unitialized) property id cannot be set into the DotvvmControlProperties dictionary.", "p");

        /// <summary> Tries to set value into the dictionary without overwriting anything. </summary>
        /// <returns> True if the value was added, false if the key was already present with a different value. </returns>
        public bool TryAdd(DotvvmProperty p, object? value) => TryAdd(p.Id, value);

        /// <summary> Tries to set value into the dictionary without overwriting anything. </summary>
        /// <returns> True if the value was added, false if the key was already present with a different value. </returns>
        public bool TryAdd(DotvvmPropertyId p, object? value)
        {
            CheckInvariant();
            if (p.MemberId == 0) ThrowZeroPropertyId();

            if (state == TableState.Array8)
            {
                Debug.Assert(values!.GetType() == typeof(object[]));
                Debug.Assert(keys is {});
                var slot = Impl.FindSlotOrFree8(this.keys, p, out var exists);
                Debug.Assert(slot < 8);
                if (slot >= 0)
                {
                    if (exists)
                    {
                        return Object.ReferenceEquals(valuesAsArrayUnsafe![slot], value);
                    }
                    OwnValues();
                    OwnKeys();
                    // arrays are always length >= 8
                    Unsafe.Add(ref Impl.UnsafeArrayReference(keys), slot) = p;
                    Unsafe.Add(ref Impl.UnsafeArrayReference(valuesAsArray), slot) = value; // avoid covariance check
                    CheckInvariant();
                    Debug.Assert(GetOrThrow(p) == value, $"{p} was not set to {value}.");
                    return true;
                }
            }
            return TryAddOulined(p, value);
        }

        private bool TryAddOulined(DotvvmPropertyId p, object? value)
        {
            TailCall:

            var keys = this.keys;
            if (keys != null)
            {
                Debug.Assert(this.values is object[]);
                Debug.Assert(keys is DotvvmPropertyId[]);
                var slot = this.state == TableState.Array8
                                ? Impl.FindSlotOrFree8(keys, p, out var exists)
                                : Impl.FindSlotOrFree16(keys, p, out exists);
                if (slot >= 0)
                {
                    if (exists)
                    {
                        // value already exists
                        return Object.ReferenceEquals(valuesAsArrayUnsafe![slot], value);
                    }
                    else
                    {
                        if (!this.ownsKeys)
                            keys = CloneKeys();
                        OwnValues();
                        keys[slot] = p;
                        var valuesAsArray = this.valuesAsArray;
                        Impl.Assert(valuesAsArray.Length > slot);
                        Unsafe.Add(ref Impl.UnsafeArrayReference(valuesAsArray), slot) = value; // avoid covariance check
                        CheckInvariant();
                        Debug.Assert(GetOrThrow(p) == value, $"{p} was not set to {value}.");
                        return true;
                    }
                }
                else
                {
                    IncreaseSize();
                    goto TailCall;
                }
            }
            if (this.values == null)
            {
                SetEmptyToSingle(p, value);
                return true;
            }
            else
            {
                // System.Dictionary backend
#if CSharp8Polyfill
                if (valuesAsDictionary.TryGetValue(p, out var existingValue))
                    return Object.ReferenceEquals(existingValue, value);
                else
                {
                    OwnValues();
                    valuesAsDictionary.Add(p, value);
                    return true;
                }
#else
                OwnValues();
                return valuesAsDictionary.TryAdd(p, value) || Object.ReferenceEquals(valuesAsDictionary[p], value);
#endif
            }
        }

        private void SetEmptyToSingle(DotvvmPropertyId p, object? value)
        {
            Debug.Assert(this.keys == null);
            Debug.Assert(this.values == null);
            Debug.Assert(this.state == TableState.Empty);
            var newKeys = new DotvvmPropertyId[Impl.AdhocTableSize];
            newKeys[0] = p;
            var newValues = new object?[Impl.AdhocTableSize];
            newValues[0] = value;

            this.keys = newKeys;
            this.values = newValues;
            this.ownsKeys = this.ownsValues = true;
            this.state = TableState.Array8;
            CheckInvariant();
        }

        [MethodImpl(Inline)]
        public readonly DotvvmControlPropertyIdEnumerator GetEnumerator()
        {
            CheckInvariant();
            if (keys != null) return new DotvvmControlPropertyIdEnumerator(keys, valuesAsArray);

            if (values == null) return EmptyEnumerator;

            return new DotvvmControlPropertyIdEnumerator(valuesAsDictionary.GetEnumerator());
        }

        readonly IEnumerator<KeyValuePair<DotvvmPropertyId, object?>> IEnumerable<KeyValuePair<DotvvmPropertyId, object?>>.GetEnumerator() =>
            GetEnumerator();

        readonly IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        [MethodImpl(Inline)]
        public readonly PropertyGroupEnumerable PropertyGroup(ushort groupId) => new(in this, groupId);

        public readonly DotvvmControlPropertyIdGroupEnumerator EnumeratePropertyGroup(ushort id) =>
            this.keys is {} keys ? new(keys, valuesAsArray, id) :
            this.values is {} ? new(valuesAsDictionary.GetEnumerator(), id) :
            default;

        private static readonly DotvvmControlPropertyIdEnumerator EmptyEnumerator = new DotvvmControlPropertyIdEnumerator(Array.Empty<DotvvmPropertyId>(), Array.Empty<object>());

        public bool Remove(DotvvmProperty key) => Remove(key.Id);
        public bool Remove(DotvvmPropertyId key)
        {
            CheckInvariant();
            if (this.keys != null)
            {
                var slot = Impl.FindSlot(this.keys, key);
                if (slot < 0)
                    return false;
                this.OwnKeys();
                this.keys[slot] = default;
                this.OwnValues();
                this.valuesAsArray[slot] = default;
                CheckInvariant();
                return true;
            }
            if (this.values == null)
                return false;
            else
            {
                Debug.Assert(values is Dictionary<DotvvmPropertyId, object>);
                return valuesAsDictionary.Remove(key);
            }
        }

        private static object? CloneValue(object? value, DotvvmBindableObject? newParent)
        {
            if (value is DotvvmBindableObject bindableObject)
            {
                bindableObject = bindableObject.CloneControl();
                bindableObject.Parent = newParent;
                return bindableObject;
            }
            return null;
        }

        public readonly int Count()
        {
            CheckInvariant();
            if (this.values == null) return 0;
            if (this.keys == null)
                return this.valuesAsDictionary.Count;
            
            return Impl.Count(this.keys);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static byte BoolToInt(bool x) => Unsafe.As<bool, byte>(ref x);

        internal void CloneInto(ref DotvvmControlProperties newDict, DotvvmBindableObject newParent)
        {
            CheckInvariant();
            if (this.values == null)
            {
                newDict = default;
                return;
            }
            else if (this.keys == null)
            {
                var dictionary = this.valuesAsDictionary;
                if (dictionary.Count > 16)
                {
                    newDict = this;
                    newDict.keys = null;
                    Dictionary<DotvvmPropertyId, object?>? newValues = null;
                    foreach (var (key, value) in dictionary)
                        if (CloneValue(value, newParent) is {} newValue)
                        {
                            if (newValues is null)
                                // ok, we have to copy it
                                newValues = new Dictionary<DotvvmPropertyId, object?>(dictionary);
                            newDict.valuesAsDictionary[key] = newValue;
                        }

                    if (newValues is null)
                    {
                        newDict.valuesAsDictionary = dictionary;
                        newDict.ownsValues = false;
                        this.ownsValues = false;
                    }
                    else
                    {
                        newDict.valuesAsDictionary = newValues;
                        newDict.ownsValues = true;
                    }
                    newDict.CheckInvariant();
                    CheckInvariant();
                    return;
                }
                // move to immutable version if it's small. It will be probably cloned multiple times again
                SwitchToSimdDict();
            }

            newDict = this;
            newDict.ownsKeys = false;
            this.ownsKeys = false;
            var valuesAsArray = newDict.valuesAsArray;
            for (int i = 0; i < valuesAsArray.Length; i++)
            {
                if (!newDict.keys![i].IsZero && valuesAsArray[i] is {} && CloneValue(valuesAsArray[i], newParent) is {} newValue)
                {
                    // clone the array if we didn't do that already
                    if (newDict.values == this.values)
                    {
                        newDict.values = valuesAsArray = valuesAsArray.AsSpan().ToArray();
                        newDict.ownsValues = true;
                    }

                    valuesAsArray[i] = newValue;
                }
            }

            if (newDict.values == this.values)
            {
                this.ownsValues = false;
                newDict.ownsValues = false;
            }
            newDict.CheckInvariant();
            CheckInvariant();
        }

        [MethodImpl(Inline)]
        void OwnKeys()
        {
            if (this.ownsKeys) return;
            CloneKeys();
        }
        [MethodImpl(Inline)]
        void OwnValues()
        {
            if (this.ownsValues) return;
            CloneValues();
        }
        [MethodImpl(NoInlining)]
        DotvvmPropertyId[] CloneKeys()
        {
            CheckInvariant();
            var oldKeys = this.keys;
            var newKeys = new DotvvmPropertyId[oldKeys!.Length];
            MemoryExtensions.CopyTo(oldKeys, newKeys.AsSpan());
            this.keys = newKeys;
            this.ownsKeys = true;
            CheckInvariant();
            return newKeys;
        }
        [MethodImpl(NoInlining)]
        void CloneValues()
        {
            CheckInvariant();
            if (keys is {})
            {
                var oldValues = this.valuesAsArray;
                var newValues = new object?[oldValues.Length];
                MemoryExtensions.CopyTo(oldValues, newValues.AsSpan());
                this.valuesAsArray = newValues;
                this.ownsValues = true;
            }
            else if (values is null)
                return;
            else
            {
                this.valuesAsDictionary = new Dictionary<DotvvmPropertyId, object?>(this.valuesAsDictionary);
                this.ownsValues = true;
            }
            CheckInvariant();
        }
        
        /// <summary> Switch Empty -> Array8, Array8 -> Array16, or Array16 -> Dictionary </summary>
        void IncreaseSize()
        {
            CheckInvariant();
            switch (state)
            {
                case TableState.Empty:
                    this.keys = new DotvvmPropertyId[Impl.AdhocTableSize];
                    this.valuesAsArray = new object?[Impl.AdhocTableSize];
                    this.state = TableState.Array8;
                    this.ownsKeys = this.ownsValues = true;
                    break;
                case TableState.Array8:
                    var newKeys = new DotvvmPropertyId[16];
                    var newValues = new object?[16];
                    MemoryExtensions.CopyTo(this.keys, newKeys.AsSpan());
                    MemoryExtensions.CopyTo(this.valuesAsArray, newValues.AsSpan());
                    this.keys = newKeys;
                    this.valuesAsArray = newValues;
                    this.state = TableState.Array16;
                    this.ownsKeys = this.ownsValues = true;
                    break;
                case TableState.Array16:
                    SwitchToDictionary();
                    break;
                case TableState.Dictinary:
                    break;
                default:
                    Impl.Fail();
                    break;
            }
            CheckInvariant();
        }

        /// <summary> Converts the internal representation to System.Collections.Generic.Dictionary </summary>
        void SwitchToDictionary()
        {
            CheckInvariant();
            switch (state)
            {
                case TableState.Array8:
                case TableState.Array16:
                    var keysTmp = this.keys;
                    var valuesTmp = this.valuesAsArray;
                    var d = new Dictionary<DotvvmPropertyId, object?>(capacity: keysTmp!.Length);

                    for (int i = 0; i < keysTmp.Length; i++)
                    {
                        if (keysTmp[i].Id != 0)
                            d[keysTmp[i]] = valuesTmp[i];
                    }
                    this.state = TableState.Dictinary;
                    this.valuesAsDictionary = d;
                    this.keys = null;
                    this.ownsValues = true;
                    break;
                case TableState.Empty:
                    this.state = TableState.Dictinary;
                    this.valuesAsDictionary = new Dictionary<DotvvmPropertyId, object?>();
                    this.ownsValues = true;
                    break;
                case TableState.Dictinary:
                    break;
                default:
                    Impl.Fail();
                    break;
            }
            CheckInvariant();
        }

        /// <summary> Converts the internal representation to the DotVVM small dictionary implementation </summary>
        void SwitchToSimdDict()
        {
            CheckInvariant();
            if (this.keys is {})
            {
                // already in the small dictionary format
                return;
            }
            else if (this.values is {})
            {
                var valuesAsDictionary = this.valuesAsDictionary;

                if (valuesAsDictionary.Count > 16)
                {
                    return;
                }

                var properties = new DotvvmPropertyId[valuesAsDictionary.Count >= 8 ? 16 : 8];
                var values = new object?[properties.Length];
                int j = 0;
                foreach (var x in valuesAsDictionary)
                {
                    (properties[j], values[j]) = x;
                    j++;
                }
                this.keys = properties;
                this.valuesAsArray = values;
                this.state = properties.Length == 8 ? TableState.Array8 : TableState.Array16;
                this.ownsKeys = true;
                this.ownsValues = true;
            }
            else
            {
            }
            CheckInvariant();
        }

        public readonly struct PropertyGroupEnumerable: IEnumerable<KeyValuePair<DotvvmPropertyId, object?>>
        {
            private readonly DotvvmControlProperties properties;
            private readonly ushort groupId;
            [MethodImpl(Inline)]
            public PropertyGroupEnumerable(in DotvvmControlProperties properties, ushort groupId)
            {
                this.properties = properties;
                this.groupId = groupId;
            }

            [MethodImpl(Inline)]
            public IEnumerator<KeyValuePair<DotvvmPropertyId, object?>> GetEnumerator() => properties.EnumeratePropertyGroup(groupId);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public enum TableState : byte
        {
            Empty = 0,
            Dictinary = 1,
            Array8 = 2,
            Array16 = 3,
        }
    }

    public struct DotvvmControlPropertyIdGroupEnumerator : IEnumerator<KeyValuePair<DotvvmPropertyId, object?>>
    {
        private readonly DotvvmPropertyId[]? keys;
        private readonly object?[]? values;
        private readonly ushort groupId;
        private readonly ushort bitmap;
        private int index;
        private Dictionary<DotvvmPropertyId, object?>.Enumerator dictEnumerator;

        internal DotvvmControlPropertyIdGroupEnumerator(DotvvmPropertyId[] keys, object?[] values, ushort groupId)
        {
            this.keys = keys;
            this.values = values;
            this.index = -1;
            this.groupId = groupId;
            this.bitmap = Impl.FindGroupBitmap(keys, groupId);
            dictEnumerator = default;
        }

        internal DotvvmControlPropertyIdGroupEnumerator(Dictionary<DotvvmPropertyId, object?>.Enumerator e, ushort groupId)
        {
            this.keys = null;
            this.values = null;
            this.index = 0;
            this.groupId = groupId;
            this.dictEnumerator = e;
        }

        public KeyValuePair<DotvvmPropertyId, object?> Current =>
            this.keys is {} keys
                ? new(keys[index]!, values![index])
                : dictEnumerator.Current;

        object IEnumerator.Current => this.Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            var keys = this.keys;
            if (keys is {})
            {
                var index = this.index + 1;
                var bitmap = this.bitmap >> index;
#if NET5_0_OR_GREATER
                this.index = index + BitOperations.TrailingZeroCount(bitmap);
#else
                while ((bitmap & 1) == 0)
                {
                    index++;
                    bitmap >>= 1;
                }
                this.index = index;
#endif
                return bitmap != 0;
            }
            else
            {
                // `default(T)` - empty collection
                if (groupId == 0)
                    return false;

                while (dictEnumerator.MoveNext())
                {
                    if (dictEnumerator.Current.Key.IsInPropertyGroup(groupId))
                        return true;
                }
                return false;
            }
        }

        public void Reset()
        {
            if (keys == null)
                ((IEnumerator)dictEnumerator).Reset();
            else index = -1;
        }
    }

    public struct DotvvmControlPropertyIdEnumerator : IEnumerator<KeyValuePair<DotvvmPropertyId, object?>>
    {
        private DotvvmPropertyId[]? keys;
        private object?[]? values;
        private int index;
        private Dictionary<DotvvmPropertyId, object?>.Enumerator dictEnumerator;

        internal DotvvmControlPropertyIdEnumerator(DotvvmPropertyId[] keys, object?[] values)
        {
            this.keys = keys;
            this.values = values;
            this.index = -1;
            dictEnumerator = default;
        }

        internal DotvvmControlPropertyIdEnumerator(Dictionary<DotvvmPropertyId, object?>.Enumerator e)
        {
            this.keys = null;
            this.values = null;
            this.index = 0;
            this.dictEnumerator = e;
        }

        public KeyValuePair<DotvvmPropertyId, object?> Current => this.keys is {} keys ? new(keys[index]!, values![index]) : dictEnumerator.Current;

        object IEnumerator.Current => this.Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            var keys = this.keys;
            if (keys is null)
                return dictEnumerator.MoveNext();
            var index = this.index;
            while (++index < keys.Length && keys[index].Id == 0) { }
            this.index = index;
            return index < keys.Length;
        }

        public void Reset()
        {
            if (keys is null)
                ((IEnumerator)dictEnumerator).Reset();
            else index = -1;
        }
    }

    public struct DotvvmControlPropertiesEnumerator : IEnumerator<KeyValuePair<DotvvmProperty, object?>>
    {
        DotvvmControlPropertyIdEnumerator idEnumerator;
        public DotvvmControlPropertiesEnumerator(DotvvmControlPropertyIdEnumerator idEnumerator)
        {
            this.idEnumerator = idEnumerator;
        }

        public KeyValuePair<DotvvmProperty, object?> Current
        {
            get
            {
                var x = idEnumerator.Current;
                return new KeyValuePair<DotvvmProperty, object?>(x.Key.PropertyInstance, x.Value);
            }
        }

        object IEnumerator.Current => this.Current;

        public void Dispose() => idEnumerator.Dispose();
        public bool MoveNext() => idEnumerator.MoveNext();
        public void Reset() => idEnumerator.Reset();
    }

    public readonly struct DotvvmPropertyDictionary : IDictionary<DotvvmProperty, object?>
    {

        private readonly DotvvmBindableObject control;

        public DotvvmPropertyDictionary(DotvvmBindableObject control)
        {
            this.control = control;
        }

        public object? this[DotvvmProperty key] { get => control.properties.GetOrThrow(key); set => control.properties.Set(key, value); }

        public KeysCollection Keys => new KeysCollection(control);
        ICollection<DotvvmProperty> IDictionary<DotvvmProperty, object?>.Keys => Keys;

        public ValuesCollection Values => new ValuesCollection(control);
        ICollection<object?> IDictionary<DotvvmProperty, object?>.Values => Values;

        public int Count => control.properties.Count();
        public bool IsReadOnly => false;

        public void Add(DotvvmProperty key, object? value)
        {
            if (!control.properties.TryAdd(key, value))
                throw new System.ArgumentException("An item with the same key has already been added.");
        }
        public bool TryAdd(DotvvmProperty key, object? value)
        {
            return control.properties.TryAdd(key, value);
        }

        public void Add(KeyValuePair<DotvvmProperty, object?> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            control.properties.ClearEverything();
        }

        public bool Contains(KeyValuePair<DotvvmProperty, object?> item) => control.properties.TryGet(item.Key, out var x) && Object.Equals(x, item.Value);

        public bool ContainsKey(DotvvmProperty key) => control.properties.Contains(key);

        public void CopyTo(KeyValuePair<DotvvmProperty, object?>[] array, int arrayIndex)
        {
            foreach (var x in this)
            {
                array[arrayIndex++] = x;
            }
        }

        public DotvvmControlPropertiesEnumerator GetEnumerator() => new(control.properties.GetEnumerator());

        public bool Remove(DotvvmProperty key)
        {
            return control.properties.Remove(key);
        }

        public bool Remove(KeyValuePair<DotvvmProperty, object?> item)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(DotvvmProperty key, out object? value) =>
            control.properties.TryGet(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        IEnumerator<KeyValuePair<DotvvmProperty, object?>> IEnumerable<KeyValuePair<DotvvmProperty, object?>>.GetEnumerator() => this.GetEnumerator();


        public readonly struct KeysCollection : ICollection<DotvvmProperty>, IReadOnlyCollection<DotvvmProperty>
        {
            private readonly DotvvmBindableObject control;

            public KeysCollection(DotvvmBindableObject control) { this.control = control; }
            public int Count => control.properties.Count();

            public bool IsReadOnly => true;
            public void Add(DotvvmProperty item) => throw new NotSupportedException("Adding a property without value doesn't make sense");
            public void Clear() => throw new NotSupportedException("Explicitly use control.Properties.Clear() instead.");
            public bool Contains(DotvvmProperty item) => control.properties.Contains(item);
            public void CopyTo(DotvvmProperty[] array, int arrayIndex)
            {
                foreach (var x in control.properties)
                {
                    array[arrayIndex++] = x.Key.PropertyInstance;
                }
            }
            public IEnumerator<DotvvmProperty> GetEnumerator()
            {
                foreach (var x in control.properties)
                {
                    yield return x.Key.PropertyInstance;
                }
            }
            public bool Remove(DotvvmProperty item) => throw new NotSupportedException("Explicitly use control.Properties.Remove() instead.");
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public struct ValuesCollection : ICollection<object?>
        {
            private readonly DotvvmBindableObject control;

            public ValuesCollection(DotvvmBindableObject control) { this.control = control; }
            public int Count => control.properties.Count();

            public bool IsReadOnly => true;
            public void Add(object? item) => throw new NotSupportedException("Adding a value without property doesn't make sense");
            public void Clear() => throw new NotSupportedException("Explicitly use control.Properties.Clear() instead.");
            public bool Contains(object? item)
            {
                foreach (var x in control.properties)
                {
                    if (Object.Equals(x.Value, item))
                        return true;
                }
                return false;
            }
            public void CopyTo(object?[] array, int arrayIndex)
            {
                foreach (var x in control.properties)
                {
                    array[arrayIndex++] = x.Value;
                }
            }
            public IEnumerator<object?> GetEnumerator()
            {
                foreach (var x in control.properties)
                {
                    yield return x.Value;
                }
            }
            public bool Remove(object? item) => throw new NotSupportedException("Explicitly use control.Properties.Remove() instead.");
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
