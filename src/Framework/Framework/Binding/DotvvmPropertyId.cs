using System;
using System.Diagnostics.CodeAnalysis;
using DotVVM.Framework.Compilation.ControlTree;

namespace DotVVM.Framework.Binding
{
    public readonly struct DotvvmPropertyId: IEquatable<DotvvmPropertyId>, IEquatable<uint>, IComparable<DotvvmPropertyId>
    {
        public readonly uint Id;
        public DotvvmPropertyId(uint id)
        {
            Id = id;
        }

        public DotvvmPropertyId(ushort typeOrGroupId, ushort memberId)
        {
            Id = ((uint)typeOrGroupId << 16) | memberId;
        }

        [MemberNotNullWhen(true, nameof(PropertyGroupInstance), nameof(GroupMemberName))]
        public bool IsPropertyGroup => (int)Id < 0;
        public ushort TypeId => (ushort)(Id >> 16);
        public ushort GroupId => (ushort)((Id >> 16) ^ 0x80_00);
        public ushort MemberId => (ushort)(Id & 0xFFFF);

        /// <summary> Returns true if the property does not have GetValue/SetValue overrides and is not inherited. That means it is sufficient  </summary>
        public bool CanUseFastAccessors
        {
            get
            {
                // properties: we encode this information as the LSB bit of the member ID (i.e. odd/even numbers)
                // property groups: always true, i.e.
                const uint mask = (1u << 31) | (1u);
                const uint targetValue = 1u;
                return (Id & mask) != targetValue;
            }
        }

        public bool IsZero => Id == 0;

        public DotvvmProperty PropertyInstance => DotvvmPropertyIdAssignment.GetProperty(Id) ?? throw new Exception($"Property with ID {Id} not registered.");
        public DotvvmPropertyGroup? PropertyGroupInstance => !IsPropertyGroup ? null : DotvvmPropertyIdAssignment.GetPropertyGroup(GroupId);
        public string? GroupMemberName => !IsPropertyGroup ? null : DotvvmPropertyIdAssignment.GetGroupMemberName(MemberId);

        public Type PropertyType => IsPropertyGroup ? PropertyGroupInstance.PropertyType : PropertyInstance.PropertyType;
        public Type DeclaringType => IsPropertyGroup ? PropertyGroupInstance.DeclaringType : DotvvmPropertyIdAssignment.GetControlType(TypeId);

        public bool IsInPropertyGroup(ushort id) => (this.Id >> 16) == ((uint)id | 0x80_00u);

        public static DotvvmPropertyId CreatePropertyGroupId(ushort groupId, ushort memberId) => new DotvvmPropertyId((ushort)(groupId | 0x80_00), memberId);

        public static implicit operator DotvvmPropertyId(uint id) => new DotvvmPropertyId(id);

        public bool Equals(DotvvmPropertyId other) => Id == other.Id;
        public bool Equals(uint other) => Id == other;
        public override bool Equals(object? obj) => obj is DotvvmPropertyId id && Equals(id);
        public override int GetHashCode() => (int)Id;

        public static bool operator ==(DotvvmPropertyId left, DotvvmPropertyId right) => left.Equals(right);
        public static bool operator !=(DotvvmPropertyId left, DotvvmPropertyId right) => !left.Equals(right);

        public override string ToString()
        {
            if (IsPropertyGroup)
            {
                var pg = PropertyGroupInstance;
                return $"[{Id:x8}]{pg.DeclaringType.Name}.{pg.Name}:{GroupMemberName}";
            }
            else
            {
                return $"[{Id:x8}]{PropertyInstance.FullName}";
            }
        }
        public int CompareTo(DotvvmPropertyId other) => Id.CompareTo(other.Id);
    }
}
