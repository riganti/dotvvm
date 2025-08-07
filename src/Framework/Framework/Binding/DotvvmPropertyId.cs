using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding
{
    /// <summary>
    /// Represents a unique <see cref="DotvvmProperty"/> ID, used as a key for <see cref="DotvvmPropertyDictionary" />.
    /// </summary>
    /// <remarks>
    /// The ID is a 32-bit unsigned integer, where:
    /// - the most significant bit indicates whether the ID is of a property group (1) or a classic property (0)
    /// - the next upper 15 bits are the <see cref="TypeId"/> (for classic properties) or <see cref="GroupId"/> (for property groups)
    /// - the lower 16 bits are the <see cref="MemberId"/>, ID of the string key for property groups or the ID of the property for classic properties
    /// - in case of classic properties, the LSB bit of the member ID indicates whether the property has GetValue/SetValue overrides and is not inherited (see <see cref="CanUseFastAccessors"/>)
    /// </remarks>
    public readonly struct DotvvmPropertyId: IEquatable<DotvvmPropertyId>, IEquatable<uint>, IComparable<DotvvmPropertyId>
    {
        /// <summary> Numeric representation of the property ID. </summary>
        public readonly uint Id;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DotvvmPropertyId(uint id)
        {
            Id = id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public DotvvmPropertyId(ushort typeOrGroupId, ushort memberId)
        {
            Id = ((uint)typeOrGroupId << 16) | memberId;
        }

        /// <summary> Returns true if the property is a <see cref="GroupedDotvvmProperty"/> other type of property. </summary>
        [MemberNotNullWhen(true, nameof(PropertyGroupInstance), nameof(GroupMemberName))]
        public bool IsPropertyGroup
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Id < 0;
        }
        /// <summary> Returns the ID of the property declaring type </summary>
        public ushort TypeId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)(Id >> 16);
        }
        /// <summary> Returns the ID of the property group. </summary>
        public ushort GroupId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)((Id >> 16) ^ 0x8000);
        }
        /// <summary> Returns the ID of the property member, i.e. property-in-type id for classic properties, or the name ID for property groups. </summary>
        public ushort MemberId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)(Id & 0xFFFF);
        }

        /// <summary> Returns true if the property does not have GetValue/SetValue overrides and is not inherited. That means it is sufficient to call properties.TryGet instead going through the DotvvmProperty.GetValue dynamic dispatch </summary>
        public bool CanUseFastAccessors
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                // properties: we encode this information as the LSB bit of the member ID (i.e. odd/even numbers)
                // property groups: always true, i.e.
                const uint mask = (1u << 31) | (1u);
                const uint targetValue = 1u;
                return (Id & mask) != targetValue;
            }
        }

        /// <summary> Returns true if the ID is default. This ID is invalid for most purposes and can be used as a sentinel value. </summary>
        public bool IsZero
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Id == 0;
        }

        /// <summary> Returns the <see cref="DotvvmProperty"/> instance for this ID. Note that a new <see cref="GroupedDotvvmProperty"/> might need to be allocated. </summary>
        public DotvvmProperty PropertyInstance => DotvvmPropertyIdAssignment.GetProperty(Id) ?? throw new Exception($"Property with ID {Id} not registered.");

        /// <summary> Returns the <see cref="GroupedDotvvmProperty"/> instance for this ID, or <c>null</c> if the ID is of a classic property. </summary>
        
        public DotvvmPropertyGroup? PropertyGroupInstance => !IsPropertyGroup ? null : DotvvmPropertyIdAssignment.GetPropertyGroup(GroupId);

        /// <summary> Returns the name (string dictionary key) of the property group member, or <c>null</c> if the ID is of a classic property. </summary>
        public string? GroupMemberName => !IsPropertyGroup ? null : DotvvmPropertyIdAssignment.GetGroupMemberName(MemberId);

        /// <summary> Returns the type of the property. </summary>
        public Type PropertyType => IsPropertyGroup ? PropertyGroupInstance.PropertyType : PropertyInstance.PropertyType;

        /// <summary> Returns the property declaring type. </summary>
        public Type DeclaringType => IsPropertyGroup ? PropertyGroupInstance.DeclaringType : DotvvmPropertyIdAssignment.GetControlType(TypeId);

        /// <summary> Returns the property declaring type. </summary>
        [MemberNotNullWhen(true, nameof(PropertyGroupInstance), nameof(GroupMemberName))]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInPropertyGroup(ushort id) => (this.Id >> 16) == ((uint)id | 0x80_00u);

        /// <summary> Constucts property ID from a property group name and a name ID. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DotvvmPropertyId CreatePropertyGroupId(ushort groupId, ushort memberId) => new DotvvmPropertyId((ushort)(groupId | 0x80_00), memberId);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator DotvvmPropertyId(uint id) => new DotvvmPropertyId(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator uint(DotvvmPropertyId id) => id.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(DotvvmPropertyId other) => Id == other.Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(uint other) => Id == other;

        public override bool Equals(object? obj) => obj is DotvvmPropertyId id && Equals(id);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (int)Id;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(DotvvmPropertyId left, DotvvmPropertyId right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(DotvvmPropertyId left, DotvvmPropertyId right) => !left.Equals(right);

        public override string ToString()
        {
            if (IsPropertyGroup)
            {
                var pg = PropertyGroupInstance;
                return $"[{TypeId:x4}_{MemberId:x4}]{pg.DeclaringType.Name}.{pg.Name}:{GroupMemberName}";
            }
            else
            {
                return $"[{TypeId:x4}_{MemberId:x4}]{PropertyInstance.FullName}";
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(DotvvmPropertyId other) => Id.CompareTo(other.Id);
    }
}
