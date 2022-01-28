using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Compilation.ControlTree.Resolved
{
	public struct PropertyGroupMember: IEquatable<PropertyGroupMember>, IPropertyGroupMember
	{
		public readonly string Name;
		public readonly DotvvmPropertyGroup  Group;

		string IPropertyGroupMember.Name => Name;
		IPropertyGroupDescriptor IPropertyGroupMember.PropertyGroup => Group;

		public PropertyGroupMember(DotvvmPropertyGroup  group, string name)
		{
			this.Group = group;
			this.Name = name;
		}

		public override bool Equals(object? obj) => obj is PropertyGroupMember && Equals((PropertyGroupMember)obj);
		public bool Equals(PropertyGroupMember other) => other.Group == Group && Name == other.Name;

		public override int GetHashCode() =>
			(Name.GetHashCode() * 17) + Group.GetHashCode();
	}
}
