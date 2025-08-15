using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;
using FastExpressionCompiler;

namespace DotVVM.Framework.Binding
{

    internal static partial class DotvvmPropertyIdAssignment
    {
        /// Type and property group IDs bellow this are reserved for manual ID assignment
        const int RESERVED_CONTROL_TYPES = 256;
        /// Properties with ID bellow this are reserved for manual ID assignment (only makes sense for controls with manual type ID)
        const int RESERVED_PROPERTY_COUNT = 32;
        const int DEFAULT_PROPERTY_COUNT = 16;
        static readonly ConcurrentDictionary<Type, ushort> typeIds = new(concurrencyLevel: 1, capacity: 256);
        private static readonly object controlTypeRegisterLock = new object();
        private static int controlCounter = RESERVED_CONTROL_TYPES; // first 256 types are reserved for DotVVM controls
        private static ControlTypeInfo[] controls = new ControlTypeInfo[1024];
        private static readonly object groupRegisterLock = new object();
        private static int groupCounter = RESERVED_CONTROL_TYPES; // first 256 types are reserved for DotVVM controls
        private static DotvvmPropertyGroup?[] propertyGroups = new DotvvmPropertyGroup[1024];
        private static ulong[] propertyGroupActiveBitmap = new ulong[1024 / 64];
        static readonly ConcurrentDictionary<string, ushort> propertyGroupMemberIds = new(concurrencyLevel: 1, capacity: 256);
        private static readonly object groupMemberRegisterLock = new object();
        static string?[] propertyGroupMemberNames = new string[1024];

        static DotvvmPropertyIdAssignment()
        {
            foreach (var (type, id) in TypeIds.List)
            {
                typeIds[type] = id;
            }
            foreach (var (name, id) in GroupMembers.List)
            {
                propertyGroupMemberIds[name] = id;
                propertyGroupMemberNames[id] = name;
            }
        }

#region Optimized metadata accessors
        /// <summary> Equivalent to <see cref="DotvvmProperty.IsValueInherited" /> </summary>
        public static bool IsInherited(DotvvmPropertyId propertyId)
        {
            if (propertyId.CanUseFastAccessors)
                return false;

            return BitmapRead(controls[propertyId.TypeId].inheritedBitmap, propertyId.MemberId);
        }

        /// <summary> Returns if the DotvvmProperty uses standard GetValue/SetValue method and we can avoid the dynamic dispatch </summary>
        /// <seealso cref="TypeCanUseAnyDirectAccess(Type)" />
        public static bool UsesStandardAccessors(DotvvmPropertyId propertyId)
        {
            if (propertyId.CanUseFastAccessors)
            {
                return true;
            }
            else
            {
                var bitmap = controls[propertyId.TypeId].standardBitmap;
                var index = propertyId.MemberId;
                return BitmapRead(bitmap, index);
            }
        }

        /// <summary> Returns if the given property is of the <see cref="ActiveDotvvmProperty"/> or <see cref="ActiveDotvvmPropertyGroup" /> type </summary>
        public static bool IsActive(DotvvmPropertyId propertyId)
        {
            Debug.Assert(GetProperty(propertyId) != null, $"Property {propertyId} not registered.");
            ulong[] bitmap;
            uint index;
            if (propertyId.IsPropertyGroup)
            {
                bitmap = propertyGroupActiveBitmap;
                index = propertyId.GroupId;
            }
            else
            {
                bitmap = controls[propertyId.TypeId].activeBitmap;
                index = propertyId.MemberId;
            }
            return BitmapRead(bitmap, index);
        }
        
        /// <summary> Returns the DotvvmProperty with a given ID, or returns null if no such property exists. New instance of <see cref="GroupedDotvvmProperty"/> might be created. </summary>
        public static DotvvmProperty? GetProperty(DotvvmPropertyId id)
        {
            if (id.IsPropertyGroup)
            {
                var groupIx = id.GroupId;
                if (groupIx >= propertyGroups.Length)
                    return null;
                var group = propertyGroups[groupIx];
                if (group is null)
                    return null;

                return group.GetDotvvmProperty(id.MemberId);
            }
            else
            {
                var typeId = id.TypeId;
                if (typeId >= controls.Length)
                    return null;
                var typeProps = controls[typeId].properties;
                if (typeProps is null)
                    return null;
                return typeProps[id.MemberId];
            }
        }

        /// <summary> Returns the <see cref="DotvvmProperty"/> or <see cref="DotvvmPropertyGroup"/> with the given id </summary>
        public static Compilation.IControlAttributeDescriptor? GetPropertyOrPropertyGroup(DotvvmPropertyId id)
        {
            if (id.IsPropertyGroup)
            {
                var groupIx = id.GroupId;
                if (groupIx >= propertyGroups.Length)
                    return null;
                return propertyGroups[groupIx];
            }
            else
            {
                var typeId = id.TypeId;
                if (typeId >= controls.Length)
                    return null;
                var typeProps = controls[typeId].properties;
                if (typeProps is null)
                    return null;
                return typeProps[id.MemberId];
            }
        }

        /// <summary> Returns the value of the property or property group. If the property is not set, returns the default value. </summary>
        public static object? GetValueRaw(DotvvmBindableObject obj, DotvvmPropertyId id, bool inherit = true)
        {
            if (id.CanUseFastAccessors)
            {
                if (obj.properties.TryGet(id, out var value))
                    return value;

                if (id.IsPropertyGroup)
                    return propertyGroups[id.GroupId]!.DefaultValue;
                else
                    return controls[id.TypeId].properties[id.MemberId]!.DefaultValue;
            }
            else
            {
                var property = controls[id.TypeId].properties[id.MemberId];
                return property!.GetValue(obj, inherit);
            }
        }

        /// <summary> Returns the value of the property or property group. If the property is not set, returns the default value. </summary>
        public static MarkupOptionsAttribute GetMarkupOptions(DotvvmPropertyId id)
        {
            if (id.IsPropertyGroup)
            {
                var groupIx = id.GroupId;
                return propertyGroups[groupIx]!.MarkupOptions;
            }
            else
            {
                var typeId = id.TypeId;
                var typeProps = controls[typeId].properties;
                return typeProps[id.MemberId]!.MarkupOptions;
            }
        }

        /// <summary> Property or property group has type assignable to IBinding and bindings should not be evaluated in GetValue </summary>
        /// <seealso cref="DotvvmProperty.IsBindingProperty"/>
        public static bool IsBindingProperty(DotvvmPropertyId id)
        {
            if (id.IsPropertyGroup)
            {
                var groupIx = id.GroupId;
                return propertyGroups[groupIx]!.IsBindingProperty;
            }
            else
            {
                var typeId = id.TypeId;
                var typeProps = controls[typeId].properties;
                return typeProps[id.MemberId]!.IsBindingProperty;
            }
        }

        public static DotvvmPropertyGroup? GetPropertyGroup(ushort id)
        {
            if (id >= propertyGroups.Length)
                return null;
            return propertyGroups[id];
        }
#endregion

#region Registration
        public static Type GetControlType(ushort id)
        {
            if (id == 0 || id >= controls.Length)
                throw new ArgumentOutOfRangeException(nameof(id), id, "Control type ID is invalid.");
            return controls[id].controlType;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort RegisterType(Type type)
        {
            if (typeIds.TryGetValue(type, out var existingId) && controls[existingId].locker is {})
                return existingId;

            return unlikely(type);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static ushort unlikely(Type type)
            {
                var types = MemoryMarshal.CreateReadOnlySpan(ref type, 1);
                Span<ushort> ids = stackalloc ushort[1];
                RegisterTypes(types, ids);
                return ids[0];
            }
        }
        public static void RegisterTypes(ReadOnlySpan<Type> types, Span<ushort> ids)
        {
            if (types.Length == 0)
                return;

            lock (controlTypeRegisterLock)
            {
                if (controlCounter + types.Length >= controls.Length)
                {
#if NET6_0_OR_GREATER
                    var nextPow2 = 1 << (BitOperations.Log2((uint)(controlCounter + types.Length)) + 1);
#else
                    var nextPow2 = types.Length * 2;
                    while (nextPow2 < controlCounter + types.Length)
                        nextPow2 *= 2;
#endif
                    VolatileResize(ref controls, nextPow2);
                }
                for (int i = 0; i < types.Length; i++)
                {
                    var type = types[i];
                    if (!typeIds.TryGetValue(type, out var id))
                    {
                        id = (ushort)controlCounter++;
                    }
                    if (controls[id].locker is null)
                    {
                        controls[id].locker = new object();
                        controls[id].controlType = type;
                        controls[id].properties = new DotvvmProperty[DEFAULT_PROPERTY_COUNT];
                        controls[id].inheritedBitmap = new ulong[(DEFAULT_PROPERTY_COUNT - 1) / 64 + 1];
                        controls[id].standardBitmap = new ulong[(DEFAULT_PROPERTY_COUNT - 1) / 64 + 1];
                        controls[id].activeBitmap = new ulong[(DEFAULT_PROPERTY_COUNT - 1) / 64 + 1];
                        if (id < RESERVED_CONTROL_TYPES)
                        {
                            controls[id].counterStandard = RESERVED_PROPERTY_COUNT;
                            controls[id].counterNonStandard = RESERVED_PROPERTY_COUNT;
                        }
                        typeIds[type] = id;
                    }
                    ids[i] = id;
                }
            }
        }

        public static DotvvmPropertyId RegisterProperty(DotvvmProperty property)
        {
            if (property.GetType() == typeof(GroupedDotvvmProperty))
                throw new ArgumentException("RegisterProperty cannot be called with GroupedDotvvmProperty!");

            var typeCanUseDirectAccess = TypeCanUseAnyDirectAccess(property.GetType());
            var canUseDirectAccess = !property.IsValueInherited && typeCanUseDirectAccess;

            var typeId = RegisterType(property.DeclaringType);
            ref ControlTypeInfo control = ref controls[typeId];
            lock (control.locker) // single control registrations are sequential anyway (most likely)
            {
                uint id;
                if (typeId < RESERVED_CONTROL_TYPES &&
                    typeof(PropertyIds).GetField(property.DeclaringType.Name + "_" + property.Name, BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is {} predefinedId)
                {
                    id = (uint)predefinedId;
                    if ((id & 0xffff) == 0)
                        throw new InvalidOperationException($"Predefined property ID of {property} cannot be 0.");
                    if (id >> 16 != typeId)
                        throw new InvalidOperationException($"Predefined property ID of {property} does not match the property declaring type ID.");
                    if ((id & 0xffff) > RESERVED_PROPERTY_COUNT)
                        throw new InvalidOperationException($"Predefined property ID of {property} is too high (there is only {RESERVED_PROPERTY_COUNT} reserved slots).");
                    if (canUseDirectAccess != (id % 2 == 0))
                        throw new InvalidOperationException($"Predefined property ID of {property} does not match the property canUseDirectAccess={canUseDirectAccess}. The ID must be {(canUseDirectAccess ? "even" : "odd")} number.");
                    id = id & 0xffff;
                }
                else if (canUseDirectAccess)
                {
                    control.counterStandard += 1;
                    id = control.counterStandard * 2;
                }
                else
                {
                    control.counterNonStandard += 1;
                    id = control.counterNonStandard * 2 + 1;
                }
                if (id > ushort.MaxValue)
                    ThrowTooManyException(property);

                // resize arrays (we hold a write lock, but others may be reading in parallel)
                while (id >= control.properties.Length)
                {
                    VolatileResize(ref control.properties, control.properties.Length * 2);
                }
                while (id / 64 >= control.inheritedBitmap.Length)
                {
                    Debug.Assert(control.inheritedBitmap.Length == control.standardBitmap.Length);
                    Debug.Assert(control.inheritedBitmap.Length == control.activeBitmap.Length);

                    VolatileResize(ref control.inheritedBitmap, control.inheritedBitmap.Length * 2);
                    VolatileResize(ref control.standardBitmap, control.standardBitmap.Length * 2);
                    VolatileResize(ref control.activeBitmap, control.activeBitmap.Length * 2);
                }

                if (property.IsValueInherited)
                    BitmapSet(control.inheritedBitmap, (uint)id);
                if (typeCanUseDirectAccess)
                    BitmapSet(control.standardBitmap, (uint)id);
                if (property is ActiveDotvvmProperty)
                    BitmapSet(control.activeBitmap, (uint)id);

                control.properties[id] = property;
                return new DotvvmPropertyId(typeId, (ushort)id);
            }

            static void ThrowTooManyException(DotvvmProperty property) =>
                throw new Exception($"Too many properties registered for a single control type ({property.DeclaringType.ToCode()}). Trying to register property '{property.Name}: {property.PropertyType.ToCode()}'");
        }

        private static readonly ConcurrentDictionary<Type, (bool getter, bool setter)> cacheTypeCanUseDirectAccess = new(concurrencyLevel: 1, capacity: 10);

        /// <summary> Does the property use the default GetValue/SetValue methods? </summary>
        public static (bool getter, bool setter) TypeCanUseDirectAccess(Type propertyType)
        {
            if (propertyType == typeof(DotvvmProperty) || propertyType == typeof(GroupedDotvvmProperty))
                return (true, true);

            if (propertyType == typeof(DotvvmCapabilityProperty) || propertyType == typeof(DotvvmPropertyAlias) || propertyType == typeof(CompileTimeOnlyDotvvmProperty))
                return (false, false);

            if (propertyType.IsGenericType)
            {
                propertyType = propertyType.GetGenericTypeDefinition();
                if (propertyType == typeof(DelegateActionProperty<>))
                    return (true, true);
            }

            return cacheTypeCanUseDirectAccess.GetOrAdd(propertyType, static t =>
            {
                var getter = t.GetMethod(nameof(DotvvmProperty.GetValue), new [] { typeof(DotvvmBindableObject), typeof(bool) })!.DeclaringType == typeof(DotvvmProperty);
                var setter = t.GetMethod(nameof(DotvvmProperty.SetValue), new [] { typeof(DotvvmBindableObject), typeof(object) })!.DeclaringType == typeof(DotvvmProperty);
                return (getter, setter);
            });
        }
        public static bool TypeCanUseAnyDirectAccess(Type propertyType)
        {
            var (getter, setter) = TypeCanUseDirectAccess(propertyType);
            return getter && setter;
        }

        public static ushort RegisterPropertyGroup(DotvvmPropertyGroup group)
        {
            lock (groupRegisterLock)
            {
                ushort id;

                // Check for predefined property group ID using reflection (similar to property registration)
                var declaringTypeId = RegisterType(group.DeclaringType);
                if (declaringTypeId < RESERVED_CONTROL_TYPES &&
                    typeof(PropertyGroupIds).GetField(group.DeclaringType.Name + "_" + group.Name, BindingFlags.Static | BindingFlags.Public)?.GetValue(null) is {} predefinedId)
                {
                    id = (ushort)predefinedId;
                    if (id == 0)
                        throw new InvalidOperationException($"Predefined property group ID of {group} cannot be 0.");
                    if (id > RESERVED_CONTROL_TYPES)
                        throw new InvalidOperationException($"Predefined property group ID of {group} is too high (there is only {RESERVED_CONTROL_TYPES} reserved slots).");
                }
                else
                {
                    id = (ushort)groupCounter++;
                    if (id == 0)
                        throw new Exception("Too many property groups registered already.");
                }

                if (id >= propertyGroups.Length)
                {
                    VolatileResize(ref propertyGroups, propertyGroups.Length * 2);
                    VolatileResize(ref propertyGroupActiveBitmap, propertyGroupActiveBitmap.Length * 2);
                }

                propertyGroups[id] = group;
                if (group is ActiveDotvvmPropertyGroup)
                    BitmapSet(propertyGroupActiveBitmap, id);
                return id;
            }
        }

        /// <summary> Thread-safe to read from the array while we are resizing </summary>
        private static void VolatileResize<T>(ref T[] array, int newSize)
        {
            var localRef = array;
            var newArray = new T[newSize];
            localRef.AsSpan().CopyTo(newArray.AsSpan(0, localRef.Length));
            Volatile.Write(ref array, newArray);
        }

#endregion Registration

#region Group members
        public static ushort GetGroupMemberId(string name, bool registerIfNotFound)
        {
            var id = GroupMembers.TryGetId(name);
            if (id != 0)
                return id;
            if (propertyGroupMemberIds.TryGetValue(name, out id))
                return id;
            if (!registerIfNotFound)
                return 0;
            return RegisterGroupMember(name);
        }

        private static ushort RegisterGroupMember(string name)
        {
            lock (groupMemberRegisterLock)
            {
                if (propertyGroupMemberIds.TryGetValue(name, out var id))
                    return id;
                id = (ushort)(propertyGroupMemberIds.Count + 1);
                if (id == 0)
                    throw new Exception("Too many property group members registered already.");
                if (id >= propertyGroupMemberNames.Length)
                    VolatileResize(ref propertyGroupMemberNames, propertyGroupMemberNames.Length * 2);
                propertyGroupMemberNames[id] = name;
                propertyGroupMemberIds[name] = id;
                return id;
            }
        }

        internal static string? GetGroupMemberName(ushort id)
        {
            if (id < propertyGroupMemberNames.Length)
                return propertyGroupMemberNames[id];
            return null;
        }
#endregion Group members

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static bool BitmapRead(ulong[] bitmap, uint index)
        {
            return (bitmap[index / 64] & (1ul << (int)(index % 64))) != 0;
        }

        static void BitmapSet(ulong[] bitmap, uint index)
        {
            bitmap[index / 64] |= 1ul << (int)(index % 64);
        }

        private struct ControlTypeInfo
        {
            public DotvvmProperty?[] properties;
            /// <summary> Bitmap for <see cref="DotvvmProperty.IsValueInherited" /> </summary>
            public ulong[] inheritedBitmap;
            /// <summary> Bitmap for <see cref="TypeCanUseAnyDirectAccess(Type)" /> </summary>
            public ulong[] standardBitmap;
            /// <summary> Bitmap storing if property is <see cref="ActiveDotvvmProperty" /> </summary>
            public ulong[] activeBitmap;
            /// TODO split struct to part used during registration and part at runtime for lookups
            public object locker;
            public Type controlType;
            public uint counterStandard;
            public uint counterNonStandard;
        }
    }
}
